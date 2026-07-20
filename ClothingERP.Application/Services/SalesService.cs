using ClothingERP.Application.Interfaces.Services;

namespace ClothingERP.Application.Services;

public class SalesService : ISalesService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILoyaltyService _loyaltySvc;
    private readonly IStockService _stock;
    private readonly IGiftCardService _giftCardSvc;
    private readonly ICommissionService _commissionSvc;
    private readonly INotificationService _notificationSvc;
    private readonly IRealtimeNotifier _realtime;
    private readonly ICurrentBranchProvider _branchProvider;

    public SalesService(IUnitOfWork uow, IMapper mapper, ILoyaltyService loyaltySvc, IStockService stock,
        IGiftCardService giftCardService, ICommissionService commissionSvc, INotificationService notificationSvc,
        IRealtimeNotifier realtime, ICurrentBranchProvider branchProvider)
        => (_uow, _mapper, _loyaltySvc, _stock, _giftCardSvc, _commissionSvc, _notificationSvc, _realtime, _branchProvider)
            = (uow, mapper, loyaltySvc, stock, giftCardService, commissionSvc, notificationSvc, realtime, branchProvider);

    public async Task<IEnumerable<SalesInvoiceListDto>> GetAllAsync()
    {
        var list = await _uow.SalesInvoices.GetQueryable()
            .Include(i => i.Customer).Where(i => !i.IsDeleted && !i.IsHold)
            .OrderByDescending(i => i.InvoiceDate).ToListAsync();
        return _mapper.Map<IEnumerable<SalesInvoiceListDto>>(list);
    }

    public async Task<SalesInvoiceDto?> GetByIdAsync(int id)
    {
        var inv = await _uow.SalesInvoices.GetWithDetailsAsync(id);
        return inv == null ? null : _mapper.Map<SalesInvoiceDto>(inv);
    }

    public async Task<ServiceResult<SalesInvoiceDto>> CreateAsync(CreateSalesInvoiceDto dto, int userId)
    {
        var branchId = _branchProvider.GetCurrentBranchId();
        await _uow.BeginTransactionAsync();
        try
        {
            // 1. Branch-aware stock validation + decrement
            var decrementedVariants = new List<(int variantId, int qty)>();
            var conflicts = new List<ConflictItemDto>();

            foreach (var item in dto.Items)
            {
                var success = await _uow.Stocks.TryDecrementAsync(item.ProductVariantId, branchId, (int)item.Quantity);
                if (!success)
                {
                    var stock = await _uow.Stocks.GetByVariantAndBranchAsync(item.ProductVariantId, branchId);
                    var variant = await _uow.ProductVariants.GetByIdAsync(item.ProductVariantId);
                    conflicts.Add(new ConflictItemDto
                    {
                        ProductVariantId = item.ProductVariantId,
                        ProductName = variant?.Product?.Name ?? "Unknown",
                        RequestedQty = (int)item.Quantity,
                        AvailableQty = (int)(stock?.Quantity ?? 0)
                    });
                    continue;
                }
                decrementedVariants.Add(((int variantId, int qty))(item.ProductVariantId, item.Quantity));
            }

            if (conflicts.Any())
            {
                // Roll back whatever was already decremented before we knew about the conflicts
                foreach (var (vid, qty) in decrementedVariants)
                    await _uow.Stocks.IncrementAsync(vid, branchId, qty);

                await _uow.RollbackTransactionAsync();

                var conflictResult = ServiceResult<SalesInvoiceDto>.Fail("পর্যাপ্ত stock নেই এই branch এ।");
                conflictResult.Conflicts = conflicts;
                return conflictResult;
            }

            decimal loyaltyDiscount = 0;

            // 2. Loyalty Points Redeem
            if (dto.CustomerId.HasValue && dto.LoyaltyPointsRedeemed > 0)
            {
                var redeemResult = await _loyaltySvc.RedeemPointsAsync(
                    dto.CustomerId.Value, dto.LoyaltyPointsRedeemed, null, userId);

                if (!redeemResult.Success)
                {
                    foreach (var (vid, qty) in decrementedVariants)
                        await _uow.Stocks.IncrementAsync(vid, branchId, qty);

                    await _uow.RollbackTransactionAsync();
                    return ServiceResult<SalesInvoiceDto>.Fail(redeemResult.Message!);
                }

                loyaltyDiscount = redeemResult.Data;
            }

            // 3. Build invoice
            var totalDiscount = dto.DiscountAmount + loyaltyDiscount;

            var invoice = new SalesInvoice
            {
                InvoiceNumber = await _uow.SalesInvoices.GenerateInvoiceNumberAsync(),
                CustomerId = dto.CustomerId,
                InvoiceDate = DateTime.UtcNow,
                Status = InvoiceStatus.Confirmed,
                DiscountAmount = totalDiscount,
                TaxAmount = dto.TaxAmount,
                IsCredit = dto.IsCredit,
                Notes = dto.Notes,
                BranchId = branchId,
                CreatedBy = userId
            };

            decimal subTotal = 0;
            foreach (var itemDto in dto.Items)
            {
                var lineTotal = (itemDto.Quantity * itemDto.UnitPrice) - itemDto.DiscountAmount + itemDto.TaxAmount;
                invoice.Items.Add(new SalesInvoiceItem
                {
                    ProductVariantId = itemDto.ProductVariantId,
                    ProductBundleId = itemDto.ProductBundleId,
                    BundleName = itemDto.BundleName,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    DiscountAmount = itemDto.DiscountAmount,
                    TaxAmount = itemDto.TaxAmount,
                    TotalAmount = lineTotal,
                    CreatedBy = userId
                });
                subTotal += itemDto.Quantity * itemDto.UnitPrice - itemDto.DiscountAmount;
            }
            invoice.SubTotal = subTotal;
            invoice.TotalAmount = subTotal - totalDiscount + dto.TaxAmount;

            await _uow.SalesInvoices.AddAsync(invoice);
            await _uow.SaveChangesAsync();

            if (invoice.TotalAmount >= 200 && !invoice.IsHold)
            {
                await _notificationSvc.CreateAsync(new CreateNotificationDto
                {
                    UserId = null,
                    Title = "Big Sale! 🎉",
                    Message = $"Invoice {invoice.InvoiceNumber} — ${invoice.TotalAmount:N2} বিক্রি হয়েছে।",
                    Type = "BigSale",
                    Severity = "success",
                    Icon = "bi-graph-up-arrow",
                    ActionUrl = $"/Sales/Details/{invoice.Id}"
                });
            }

            if (!invoice.IsHold && invoice.Status != InvoiceStatus.Cancelled)
            {
                await _commissionSvc.CalculateAndRecordCommissionAsync(
                    userId, invoice.Id, invoice.TotalAmount, userId);
            }

            // 4. Payments
            decimal totalPaid = 0;
            foreach (var payDto in dto.Payments)
            {
                if (!string.IsNullOrEmpty(payDto.GiftCardCode))
                {
                    var redeemResult = await _giftCardSvc.RedeemAsync(
                        payDto.GiftCardCode, payDto.Amount, invoice.Id, userId);

                    if (!redeemResult.Success)
                    {
                        await _uow.RollbackTransactionAsync();
                        return ServiceResult<SalesInvoiceDto>.Fail($"Gift card error: {redeemResult.Message}");
                    }
                }

                await _uow.SalesPayments.AddAsync(new SalesPayment
                {
                    SalesInvoiceId = invoice.Id,
                    CustomerId = dto.CustomerId,
                    PaymentMethod = payDto.PaymentMethod,
                    Amount = payDto.Amount,
                    PaymentDate = DateTime.UtcNow,
                    CreatedBy = userId
                });
                totalPaid += payDto.Amount;
            }

            invoice.PaidAmount = totalPaid;
            invoice.Status = totalPaid >= invoice.TotalAmount
                ? InvoiceStatus.FullyPaid
                : totalPaid > 0 ? InvoiceStatus.PartiallyPaid : InvoiceStatus.Confirmed;

            _uow.SalesInvoices.Update(invoice);
            await _uow.SaveChangesAsync();

            // 5. Stock already decremented in step 1 — just read the current
            //    branch-specific quantity for the realtime/low-stock broadcasts.
            foreach (var item in invoice.Items)
            {
                var updatedStock = await _uow.Stocks.GetByVariantAndBranchAsync(item.ProductVariantId, branchId);
                var variant = await _uow.ProductVariants.GetByIdAsync(item.ProductVariantId);

                await _realtime.NotifyStockUpdatedAsync(
                    item.ProductVariantId,
                    variant?.Barcode ?? "",
                    (int)(updatedStock?.Quantity ?? 0),
                    variant?.Product?.Name ?? "Product"
                );

                if (updatedStock != null && variant != null && updatedStock.Quantity <= variant.Product.ReorderPoint)
                {
                    await _realtime.NotifyLowStockAsync(
                        item.ProductVariantId, variant.Product.Name, (int)updatedStock.Quantity);
                }
            }

            // 6. Customer ledger if credit sale
            if (dto.IsCredit && dto.CustomerId.HasValue)
            {
                var due = invoice.TotalAmount - totalPaid;
                if (due > 0) await AddCustomerLedgerEntry(dto.CustomerId.Value,
                    LedgerEntryType.Invoice, due, 0, invoice.InvoiceNumber,
                    $"Invoice {invoice.InvoiceNumber}", userId);
            }

            // 7. Update customer total purchase and award loyalty points
            if (dto.CustomerId.HasValue)
            {
                var customer = await _uow.Customers.GetByIdAsync(dto.CustomerId.Value);
                if (customer != null)
                {
                    customer.TotalPurchaseAmount += invoice.TotalAmount;
                    customer.UpdatedBy = userId;
                    customer.UpdatedAt = DateTime.UtcNow;
                    _uow.Customers.Update(customer);
                }

                await _loyaltySvc.AwardPointsAsync(
                    dto.CustomerId.Value, invoice.TotalAmount, invoice.Id, userId);
            }

            await _uow.SaveChangesAsync();
            await _uow.CommitTransactionAsync();


            await _realtime.NotifySaleCompletedAsync(invoice.TotalAmount, invoice.InvoiceNumber);

            var result = await _uow.SalesInvoices.GetWithDetailsAsync(invoice.Id);
            return ServiceResult<SalesInvoiceDto>.Ok(_mapper.Map<SalesInvoiceDto>(result!), "Invoice created.");
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync();
            return ServiceResult<SalesInvoiceDto>.Fail($"Failed: {ex.Message}");
        }
    }
    public async Task<ServiceResult> CancelAsync(int id, string reason, int userId)
    {
        var inv = await _uow.SalesInvoices.GetWithDetailsAsync(id);
        if (inv == null) return ServiceResult.Fail("Invoice not found.");
        if (inv.Status == InvoiceStatus.Cancelled) return ServiceResult.Fail("Already cancelled.");

        await _uow.BeginTransactionAsync();
        try
        {
            inv.Status = InvoiceStatus.Cancelled;
            inv.Notes = $"{inv.Notes} | Cancelled: {reason}";
            inv.UpdatedBy = userId;
            _uow.SalesInvoices.Update(inv);

            // Restore stock to the branch the sale was made from + realtime broadcast
            foreach (var item in inv.Items)
            {
                await _uow.Stocks.IncrementAsync(item.ProductVariantId, inv.BranchId, (int)item.Quantity);

                var updatedStock = await _uow.Stocks.GetByVariantAndBranchAsync(item.ProductVariantId, inv.BranchId);
                var variant = await _uow.ProductVariants.GetByIdAsync(item.ProductVariantId);

                await _realtime.NotifyStockUpdatedAsync(
                    item.ProductVariantId,
                    variant?.Barcode ?? "",
                    (int)(updatedStock?.Quantity ?? 0),
                    variant?.Product?.Name ?? "");
            }

            await _uow.SaveChangesAsync();

            await _commissionSvc.ReverseCommissionAsync(id, userId);

            await _uow.CommitTransactionAsync();
            return ServiceResult.Ok("Invoice cancelled.");
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync();
            return ServiceResult.Fail(ex.Message);
        }
    }

    public async Task<ServiceResult> HoldAsync(int id, int userId)
    {
        var inv = await _uow.SalesInvoices.GetByIdAsync(id);
        if (inv == null) return ServiceResult.Fail("Not found.");
        inv.IsHold = true; inv.Status = InvoiceStatus.Hold; inv.UpdatedBy = userId;
        _uow.SalesInvoices.Update(inv); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Invoice put on hold.");
    }

    public async Task<ServiceResult> UnholdAsync(int id, int userId)
    {
        var inv = await _uow.SalesInvoices.GetByIdAsync(id);
        if (inv == null) return ServiceResult.Fail("Not found.");
        inv.IsHold = false; inv.Status = InvoiceStatus.Confirmed; inv.UpdatedBy = userId;
        _uow.SalesInvoices.Update(inv); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Invoice resumed.");
    }

    public async Task<IEnumerable<SalesInvoiceListDto>> GetHeldAsync()
        => _mapper.Map<IEnumerable<SalesInvoiceListDto>>(await _uow.SalesInvoices.GetHeldInvoicesAsync());

    public async Task<ServiceResult> AddPaymentAsync(int invoiceId, CreateSalesPaymentDto dto, int userId)
    {
        var inv = await _uow.SalesInvoices.GetByIdAsync(invoiceId);
        if (inv == null) return ServiceResult.Fail("Invoice not found.");

        await _uow.SalesPayments.AddAsync(new SalesPayment
        {
            SalesInvoiceId = invoiceId,
            CustomerId = inv.CustomerId,
            PaymentMethod = dto.PaymentMethod,
            Amount = dto.Amount,
            ReferenceNumber = dto.ReferenceNumber,
            PaymentDate = DateTime.UtcNow,
            CreatedBy = userId
        });

        inv.PaidAmount += dto.Amount;
        inv.Status = inv.PaidAmount >= inv.TotalAmount
            ? InvoiceStatus.FullyPaid : InvoiceStatus.PartiallyPaid;
        inv.UpdatedBy = userId;
        _uow.SalesInvoices.Update(inv);

        if (inv.IsCredit && inv.CustomerId.HasValue)
            await AddCustomerLedgerEntry(inv.CustomerId.Value, LedgerEntryType.Payment,
                0, dto.Amount, inv.InvoiceNumber, $"Payment for {inv.InvoiceNumber}", userId);

        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Payment recorded.");
    }

    public async Task<decimal> GetTodaySalesAsync(int? branchId = null)
        => await _uow.SalesInvoices.GetTodaySalesAmountAsync(branchId);
    public async Task<decimal> GetTodayProfitAsync(int? branchId = null)
        => await _uow.SalesInvoices.GetTodayProfitAsync(branchId);
    public async Task<int> GetTodayInvoiceCountAsync(int? branchId = null)
        => await _uow.SalesInvoices.GetTodayInvoiceCountAsync(branchId);

    // ── Private Helpers ───────────────────────────────────────────────────
    private async Task AddCustomerLedgerEntry(int customerId, LedgerEntryType type,
        decimal debit, decimal credit, string? reference, string description, int userId)
    {
        var currentBal = await _uow.CustomerLedgers.GetCurrentBalanceAsync(customerId);
        var newBal = currentBal + debit - credit;

        await _uow.CustomerLedgers.AddAsync(new CustomerLedger
        {
            CustomerId = customerId,
            EntryType = type,
            Debit = debit,
            Credit = credit,
            Balance = newBal,
            ReferenceNumber = reference,
            Description = description,
            EntryDate = DateTime.UtcNow,
            CreatedBy = userId
        });

        var customer = await _uow.Customers.GetByIdAsync(customerId);
        if (customer != null)
        { customer.CurrentBalance = newBal; customer.UpdatedBy = userId; _uow.Customers.Update(customer); }
    }
}