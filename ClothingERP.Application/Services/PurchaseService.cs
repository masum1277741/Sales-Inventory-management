namespace ClothingERP.Application.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IStockService _stock;

    public PurchaseService(IUnitOfWork uow, IMapper mapper, IStockService stock)
        => (_uow, _mapper, _stock) = (uow, mapper, stock);

    public async Task<IEnumerable<PurchaseOrderListDto>> GetAllOrdersAsync()
    {
        var list = await _uow.PurchaseOrders.GetQueryable()
            .Include(po => po.Supplier).Where(po => !po.IsDeleted)
            .OrderByDescending(po => po.OrderDate).ToListAsync();
        return _mapper.Map<IEnumerable<PurchaseOrderListDto>>(list);
    }

    public async Task<PurchaseOrderDto?> GetOrderByIdAsync(int id)
    {
        var po = await _uow.PurchaseOrders.GetWithDetailsAsync(id);
        return po == null ? null : _mapper.Map<PurchaseOrderDto>(po);
    }

    public async Task<ServiceResult<PurchaseOrderDto>> CreateOrderAsync(CreatePurchaseOrderDto dto, int userId)
    {
        var po = new PurchaseOrder
        {
            PONumber = await _uow.PurchaseOrders.GeneratePONumberAsync(),
            SupplierId = dto.SupplierId,
            OrderDate = DateTime.UtcNow,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            Status = PurchaseOrderStatus.Draft,
            DiscountAmount = dto.DiscountAmount,
            TaxAmount = dto.TaxAmount,
            ShippingCost = dto.ShippingCost,
            Notes = dto.Notes,
            CreatedBy = userId
        };

        decimal subTotal = 0;
        foreach (var itemDto in dto.Items)
        {
            var lineTotal = (itemDto.Quantity * itemDto.UnitCost) - itemDto.DiscountAmount;
            po.Items.Add(new PurchaseOrderItem
            {
                ProductVariantId = itemDto.ProductVariantId,
                OrderedQuantity = itemDto.Quantity,
                UnitCost = itemDto.UnitCost,
                DiscountAmount = itemDto.DiscountAmount,
                TotalCost = lineTotal,
                CreatedBy = userId
            });
            subTotal += lineTotal;
        }
        po.SubTotal = subTotal;
        po.TotalAmount = subTotal + dto.TaxAmount + dto.ShippingCost - dto.DiscountAmount;

        await _uow.PurchaseOrders.AddAsync(po);
        await _uow.SaveChangesAsync();

        // Supplier ledger entry
        await AddSupplierLedgerEntry(po.SupplierId, LedgerEntryType.Invoice,
            0, po.TotalAmount, po.PONumber, $"PO {po.PONumber}", userId);
        await _uow.SaveChangesAsync();

        var result = await _uow.PurchaseOrders.GetWithDetailsAsync(po.Id);
        return ServiceResult<PurchaseOrderDto>.Ok(_mapper.Map<PurchaseOrderDto>(result!), "Purchase order created.");
    }

    public async Task<ServiceResult<PurchaseOrderDto>> UpdateOrderAsync(int id, CreatePurchaseOrderDto dto, int userId)
    {
        var po = await _uow.PurchaseOrders.GetWithDetailsAsync(id);
        if (po == null) return ServiceResult<PurchaseOrderDto>.Fail("Not found.");
        if (po.Status != PurchaseOrderStatus.Draft) return ServiceResult<PurchaseOrderDto>.Fail("Only draft orders can be edited.");

        po.SupplierId = dto.SupplierId; po.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
        po.DiscountAmount = dto.DiscountAmount; po.TaxAmount = dto.TaxAmount;
        po.ShippingCost = dto.ShippingCost; po.Notes = dto.Notes; po.UpdatedBy = userId;

        // Remove old items
        foreach (var item in po.Items) _uow.StockMovements.Remove(new StockMovement { Id = item.Id }); // just marks
        po.Items.Clear();

        decimal subTotal = 0;
        foreach (var itemDto in dto.Items)
        {
            var lineTotal = (itemDto.Quantity * itemDto.UnitCost) - itemDto.DiscountAmount;
            po.Items.Add(new PurchaseOrderItem
            {
                ProductVariantId = itemDto.ProductVariantId,
                OrderedQuantity = itemDto.Quantity,
                UnitCost = itemDto.UnitCost,
                DiscountAmount = itemDto.DiscountAmount,
                TotalCost = lineTotal,
                CreatedBy = userId
            });
            subTotal += lineTotal;
        }
        po.SubTotal = subTotal;
        po.TotalAmount = subTotal + dto.TaxAmount + dto.ShippingCost - dto.DiscountAmount;
        _uow.PurchaseOrders.Update(po);
        await _uow.SaveChangesAsync();

        var result = await _uow.PurchaseOrders.GetWithDetailsAsync(id);
        return ServiceResult<PurchaseOrderDto>.Ok(_mapper.Map<PurchaseOrderDto>(result!), "Updated.");
    }

    public async Task<ServiceResult> ApproveOrderAsync(int id, int userId)
    {
        var po = await _uow.PurchaseOrders.GetByIdAsync(id);
        if (po == null) return ServiceResult.Fail("Not found.");
        if (po.Status != PurchaseOrderStatus.Draft) return ServiceResult.Fail("Only draft orders can be approved.");
        po.Status = PurchaseOrderStatus.Approved; po.UpdatedBy = userId;
        _uow.PurchaseOrders.Update(po); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Order approved.");
    }

    public async Task<ServiceResult> CancelOrderAsync(int id, string reason, int userId)
    {
        var po = await _uow.PurchaseOrders.GetByIdAsync(id);
        if (po == null) return ServiceResult.Fail("Not found.");
        if (po.Status == PurchaseOrderStatus.FullyReceived) return ServiceResult.Fail("Cannot cancel fully received order.");
        po.Status = PurchaseOrderStatus.Cancelled;
        po.Notes = $"{po.Notes} | Cancelled: {reason}"; po.UpdatedBy = userId;
        _uow.PurchaseOrders.Update(po); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Cancelled.");
    }

    public async Task<IEnumerable<GRNListDto>> GetAllGRNsAsync()
    {
        var list = await _uow.GoodsReceiptNotes.GetQueryable()
            .Include(g => g.PurchaseOrder).Include(g => g.Supplier)
            .Include(g => g.Items.Where(i => !i.IsDeleted))
            .Where(g => !g.IsDeleted).OrderByDescending(g => g.ReceivedDate).ToListAsync();
        return _mapper.Map<IEnumerable<GRNListDto>>(list);
    }

    public async Task<GRNDto?> GetGRNByIdAsync(int id)
    {
        var grn = await _uow.GoodsReceiptNotes.GetWithDetailsAsync(id);
        return grn == null ? null : _mapper.Map<GRNDto>(grn);
    }

    public async Task<ServiceResult<GRNDto>> CreateGRNAsync(CreateGRNDto dto, int userId)
    {
        await _uow.BeginTransactionAsync();
        try
        {
            var po = await _uow.PurchaseOrders.GetWithDetailsAsync(dto.PurchaseOrderId);
            if (po == null) return ServiceResult<GRNDto>.Fail("Purchase order not found.");

            var grn = new GoodsReceiptNote
            {
                GRNNumber = await _uow.GoodsReceiptNotes.GenerateGRNNumberAsync(),
                PurchaseOrderId = dto.PurchaseOrderId,
                SupplierId = po.SupplierId,
                ReceivedDate = DateTime.UtcNow,
                DeliveryChallan = dto.DeliveryChallan,
                Notes = dto.Notes,
                CreatedBy = userId
            };

            foreach (var itemDto in dto.Items)
            {
                grn.Items.Add(new GoodsReceiptNoteItem
                {
                    ProductVariantId = itemDto.ProductVariantId,
                    PurchaseOrderItemId = itemDto.PurchaseOrderItemId,
                    ReceivedQuantity = itemDto.ReceivedQuantity,
                    UnitCost = itemDto.UnitCost,
                    TotalCost = itemDto.ReceivedQuantity * itemDto.UnitCost,
                    CreatedBy = userId
                });

                // Update received quantity on PO item
                var poItem = po.Items.FirstOrDefault(i => i.Id == itemDto.PurchaseOrderItemId);
                if (poItem != null)
                {
                    poItem.ReceivedQuantity += itemDto.ReceivedQuantity;
                    _uow.PurchaseOrders.Update(po);
                }

                // Update stock
                await _stock.UpdateStockAsync(itemDto.ProductVariantId, itemDto.ReceivedQuantity,
                    StockMovementType.Purchase, grn.GRNNumber, userId);
            }

            await _uow.GoodsReceiptNotes.AddAsync(grn);
            await _uow.SaveChangesAsync();

            // Update PO status
            var allReceived = po.Items.All(i => i.ReceivedQuantity >= i.OrderedQuantity);
            po.Status = allReceived ? PurchaseOrderStatus.FullyReceived : PurchaseOrderStatus.PartiallyReceived;
            _uow.PurchaseOrders.Update(po);
            await _uow.SaveChangesAsync();
            await _uow.CommitTransactionAsync();

            var result = await _uow.GoodsReceiptNotes.GetWithDetailsAsync(grn.Id);
            return ServiceResult<GRNDto>.Ok(_mapper.Map<GRNDto>(result!), "GRN created.");
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync();
            return ServiceResult<GRNDto>.Fail($"Failed: {ex.Message}");
        }
    }

    public async Task<ServiceResult> AddSupplierPaymentAsync(int purchaseOrderId, decimal amount,
        PaymentMethod method, string? reference, int userId)
    {
        var po = await _uow.PurchaseOrders.GetByIdAsync(purchaseOrderId);
        if (po == null) return ServiceResult.Fail("Not found.");
        po.PaidAmount += amount; po.UpdatedBy = userId;
        _uow.PurchaseOrders.Update(po);
        await AddSupplierLedgerEntry(po.SupplierId, LedgerEntryType.Payment,
            amount, 0, po.PONumber, $"Payment for {po.PONumber}", userId);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Payment recorded.");
    }

    private async Task AddSupplierLedgerEntry(int supplierId, LedgerEntryType type,
        decimal debit, decimal credit, string? reference, string description, int userId)
    {
        var currentBal = await _uow.SupplierLedgers.GetCurrentBalanceAsync(supplierId);
        var newBal = currentBal + credit - debit;
        await _uow.SupplierLedgers.AddAsync(new SupplierLedger
        {
            SupplierId = supplierId,
            EntryType = type,
            Debit = debit,
            Credit = credit,
            Balance = newBal,
            ReferenceNumber = reference,
            Description = description,
            EntryDate = DateTime.UtcNow,
            CreatedBy = userId
        });
        var supplier = await _uow.Suppliers.GetByIdAsync(supplierId);
        if (supplier != null) { supplier.CurrentBalance = newBal; _uow.Suppliers.Update(supplier); }
    }
}