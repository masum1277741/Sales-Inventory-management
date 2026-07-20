namespace ClothingERP.Application.Services;

public class OnlineOrderService : IOnlineOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly IExchangeRateService _rateSvc;       // Feature #14
    private readonly IStorefrontService _storefrontSvc;
    private readonly INotificationService _notificationSvc; // Feature #7
    private readonly IRealtimeNotifier? _realtime;          // Feature #13 (optional)

    public OnlineOrderService(IUnitOfWork uow, IExchangeRateService rateSvc,
        IStorefrontService storefrontSvc, INotificationService notificationSvc, IRealtimeNotifier? realtime = null)
        => (_uow, _rateSvc, _storefrontSvc, _notificationSvc, _realtime) = (uow, rateSvc, storefrontSvc, notificationSvc, realtime);

    // ── Cart Pricing (checkout পেইজে দাম + stock validate করে দেখানোর জন্য) ──
    public async Task<CartPricingResultDto> PriceCartAsync(CartPricingRequestDto dto, string currency)
    {
        var result = new CartPricingResultDto();
        var settings = await _storefrontSvc.GetSettingsAsync();

        foreach (var item in dto.Items)
        {
            var variant = await _uow.ProductVariants.GetQueryable()
                .Include(v => v.Product).Include(v => v.Size).Include(v => v.Color).Include(v => v.Stock)
                .FirstOrDefaultAsync(v => v.Id == item.VariantId && v.IsActive && !v.IsDeleted);

            if (variant == null)
            {
                result.Warnings.Add($"একটা item আর available নেই (ID: {item.VariantId})।");
                continue;
            }

            var available = variant.Stock?.Quantity ?? 0;
            var unitPrice = variant.RetailPriceOverride ?? variant.Product.RetailPrice;
            var qty = Math.Min(item.Quantity, Math.Max(0, available));

            if (qty < item.Quantity)
                result.Warnings.Add($"{variant.Product.Name} ({variant.Size.Name}/{variant.Color.Name}) — মাত্র {available} টা stock এ আছে।");

            result.Lines.Add(new CartLineDto
            {
                VariantId = variant.Id,
                ProductName = variant.Product.Name,
                SizeName = variant.Size.Name,
                ColorName = variant.Color.Name,
                Quantity = item.Quantity,
                AvailableQty = (int)available,
                UnitPriceUSD = unitPrice,
                LineTotalUSD = unitPrice * item.Quantity,
                IsAvailable = available >= item.Quantity
            });
        }

        result.SubtotalUSD = result.Lines.Sum(l => l.LineTotalUSD);
        result.ShippingFeeUSD = result.SubtotalUSD >= settings.FreeShippingThresholdUSD ? 0 : settings.FlatShippingFeeUSD;
        result.TotalUSD = result.SubtotalUSD + result.ShippingFeeUSD;

        return result;
    }

    // ── Checkout — Order তৈরি + Atomic Stock Decrement (Feature #13/#21 pattern) ──
    public async Task<ServiceResult<OrderConfirmationDto>> CheckoutAsync(CheckoutDto dto, int? customerId, int? userId)
    {
        if (!dto.Items.Any()) return ServiceResult<OrderConfirmationDto>.Fail("Cart খালি।");

        var settings = await _storefrontSvc.GetSettingsAsync();
        if (!settings.IsStoreEnabled)
            return ServiceResult<OrderConfirmationDto>.Fail("Online store বর্তমানে বন্ধ আছে।");

        var pricing = await PriceCartAsync(new CartPricingRequestDto { Items = dto.Items }, dto.Currency);
        var unavailable = pricing.Lines.Where(l => !l.IsAvailable).ToList();
        if (unavailable.Any())
        {
            return ServiceResult<OrderConfirmationDto>.Fail(
                $"কিছু item এর পর্যাপ্ত stock নেই: {string.Join(", ", unavailable.Select(u => u.ProductName))}");
        }

        // ── Atomic Stock Decrement (branch-aware, fulfillment branch থেকে) ──────
        var branchId = settings.FulfillmentBranchId;
        var decremented = new List<(int variantId, int qty)>();

        foreach (var line in pricing.Lines)
        {
            var success = await _uow.Stocks.TryDecrementAsync(line.VariantId, branchId, line.Quantity);
            if (!success)
            {
                foreach (var (vid, qty) in decremented) await _uow.Stocks.IncrementAsync(vid, branchId, qty);
                return ServiceResult<OrderConfirmationDto>.Fail(
                    $"'{line.ProductName}' একই সময়ে অন্য কেউ কিনে ফেলেছে — অনুগ্রহ করে cart আপডেট করুন।");
            }
            decremented.Add((line.VariantId, line.Quantity));
        }

        // ── Order তৈরি করো ────────────────────────────────────────────────────
        var order = new OnlineOrder
        {
            OrderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
            CustomerId = customerId,
            GuestName = dto.Name,
            GuestPhone = dto.Phone,
            GuestEmail = dto.Email,
            ShippingAddress = dto.ShippingAddress,
            ShippingCity = dto.ShippingCity,
            ShippingNotes = dto.ShippingNotes,
            SubtotalUSD = pricing.SubtotalUSD,
            ShippingFeeUSD = pricing.ShippingFeeUSD,
            TotalUSD = pricing.TotalUSD,
            Currency = dto.Currency,
            PaymentMethod = dto.PaymentMethod,
            PaymentStatus = dto.PaymentMethod == "COD" ? "Pending" : "Pending",
            Status = "Placed",
            FulfillmentBranchId = branchId,
            CreatedBy = userId
        };
        await _uow.OnlineOrders.AddAsync(order);
        await _uow.SaveChangesAsync();

        foreach (var line in pricing.Lines)
        {
            await _uow.OnlineOrderItems.AddAsync(new OnlineOrderItem
            {
                OnlineOrderId = order.Id,
                ProductVariantId = line.VariantId,
                ProductName = line.ProductName,
                SizeName = line.SizeName,
                ColorName = line.ColorName,
                Quantity = line.Quantity,
                UnitPriceUSD = line.UnitPriceUSD,
                LineTotalUSD = line.LineTotalUSD
            });

            // Real-time stock broadcast (Feature #13) — admin POS এও সাথে সাথে দেখাবে
            if (_realtime != null)
            {
                var stock = await _uow.Stocks.GetByVariantAndBranchAsync(line.VariantId, branchId);
                await _realtime.NotifyStockUpdatedAsync(line.VariantId, "", (int)(stock?.Quantity ?? 0), line.ProductName);
            }
        }
        await _uow.SaveChangesAsync();

        // ── Admin কে notify করো (Feature #7) ─────────────────────────────────
        await _notificationSvc.CreateAsync(new CreateNotificationDto
        {
            UserId = null,
            Title = "নতুন Online Order! 🛍️",
            Message = $"{order.OrderNumber} — {dto.Name} — ${order.TotalUSD:N2}",
            Type = "OnlineOrder",
            Severity = "success",
            Icon = "bi-bag-check",
            ActionUrl = $"/OnlineOrder/Details/{order.Id}"
        });

        return ServiceResult<OrderConfirmationDto>.Ok(new OrderConfirmationDto
        {
            OrderNumber = order.OrderNumber,
            TotalUSD = order.TotalUSD,
            PaymentMethod = order.PaymentMethod
        }, "Order সফলভাবে placed হয়েছে!");
    }

    // ══════════════════════════════════════════════════════════════════════
    // ADMIN — Order Management
    // ══════════════════════════════════════════════════════════════════════
    public async Task<IEnumerable<OnlineOrderListDto>> GetAllAsync(string? statusFilter = null)
    {
        var query = _uow.OnlineOrders.GetQueryable()
            .Include(o => o.Customer).Include(o => o.Items)
            .Where(o => !o.IsDeleted);

        if (!string.IsNullOrEmpty(statusFilter))
            query = query.Where(o => o.Status == statusFilter);

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();

        return orders.Select(o => new OnlineOrderListDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            OrderDate = o.CreatedAt,
            CustomerName = o.Customer?.Name ?? o.GuestName,
            CustomerPhone = o.Customer?.PhoneNumber ?? o.GuestPhone,
            TotalUSD = o.TotalUSD,
            Status = o.Status,
            ItemCount = o.Items.Count,
            PaymentMethod = o.PaymentMethod,
            PaymentStatus = o.PaymentStatus
        });
    }

    public async Task<OnlineOrderDetailDto?> GetByIdAsync(int id)
    {
        var o = await _uow.OnlineOrders.GetQueryable()
            .Include(x => x.Customer).Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (o == null) return null;

        return new OnlineOrderDetailDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            OrderDate = o.CreatedAt,
            CustomerName = o.Customer?.Name ?? o.GuestName,
            CustomerPhone = o.Customer?.PhoneNumber ?? o.GuestPhone,
            TotalUSD = o.TotalUSD,
            Status = o.Status,
            ItemCount = o.Items.Count,
            PaymentMethod = o.PaymentMethod,
            PaymentStatus = o.PaymentStatus,
            ShippingAddress = o.ShippingAddress,
            ShippingCity = o.ShippingCity,
            ShippingNotes = o.ShippingNotes,
            CancellationReason = o.CancellationReason,
            Items = o.Items.Select(i => new OnlineOrderItemDto
            {
                ProductName = i.ProductName,
                SizeName = i.SizeName,
                ColorName = i.ColorName,
                Quantity = i.Quantity,
                UnitPriceUSD = i.UnitPriceUSD,
                LineTotalUSD = i.LineTotalUSD
            }).ToList()
        };
    }

    public async Task<ServiceResult> UpdateStatusAsync(UpdateOrderStatusDto dto, int userId)
    {
        var order = await _uow.OnlineOrders.GetByIdAsync(dto.OrderId);
        if (order == null) return ServiceResult.Fail("Order not found.");


        if (dto.Status == "Cancelled" && order.Status != "Cancelled")
        {
            var items = await _uow.OnlineOrderItems.GetQueryable()
                .Where(i => i.OnlineOrderId == order.Id && !i.IsDeleted).ToListAsync();
            foreach (var item in items)
                await _uow.Stocks.IncrementAsync(item.ProductVariantId, order.FulfillmentBranchId ?? 1, item.Quantity);

            order.CancellationReason = dto.CancellationReason;
        }

        order.Status = dto.Status;
        if (dto.Status == "Confirmed") order.ConfirmedAt = DateTime.UtcNow;
        if (dto.Status == "Shipped") order.ShippedAt = DateTime.UtcNow;
        if (dto.Status == "Delivered") { order.DeliveredAt = DateTime.UtcNow; order.PaymentStatus = "Paid"; }

        order.UpdatedBy = userId; order.UpdatedAt = DateTime.UtcNow;
        _uow.OnlineOrders.Update(order);
        await _uow.SaveChangesAsync();

        return ServiceResult.Ok($"Order status আপডেট হলো: {dto.Status}");
    }
}