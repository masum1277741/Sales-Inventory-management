namespace ClothingERP.Application.Services;

public class ReorderService : IReorderService
{
    private readonly IUnitOfWork _uow;

    public ReorderService(IUnitOfWork uow) => _uow = uow;
    private readonly ICurrentBranchProvider _branchProvider;
    // ── Settings ──────────────────────────────────────────────────────────
    public async Task<ReorderSettingsDto> GetSettingsAsync()
    {
        var settings = (await _uow.ReorderSettings.GetAllAsync()).FirstOrDefault();
        if (settings == null)
        {
            settings = new ReorderSettings();
            await _uow.ReorderSettings.AddAsync(settings);
            await _uow.SaveChangesAsync();
        }

        return new ReorderSettingsDto
        {
            AnalysisPeriodDays = settings.AnalysisPeriodDays,
            DefaultLeadTimeDays = settings.DefaultLeadTimeDays,
            SafetyStockDays = settings.SafetyStockDays,
            MinDailyVelocity = settings.MinDailyVelocity
        };
    }

    public async Task<ServiceResult> UpdateSettingsAsync(UpdateReorderSettingsDto dto, int userId)
    {
        var settings = (await _uow.ReorderSettings.GetAllAsync()).FirstOrDefault();
        if (settings == null) { settings = new ReorderSettings(); await _uow.ReorderSettings.AddAsync(settings); }

        settings.AnalysisPeriodDays = dto.AnalysisPeriodDays;
        settings.DefaultLeadTimeDays = dto.DefaultLeadTimeDays;
        settings.SafetyStockDays = dto.SafetyStockDays;
        settings.MinDailyVelocity = dto.MinDailyVelocity;
        settings.UpdatedBy = userId;
        settings.UpdatedAt = DateTime.UtcNow;

        _uow.ReorderSettings.Update(settings);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Reorder settings updated successfully.");
    }

   
    public async Task<List<ReorderSuggestionDto>> GetSuggestionsAsync()
    {
        var settings = await GetSettingsAsync();
        var since = DateTime.UtcNow.AddDays(-settings.AnalysisPeriodDays);

        
        var branchId = _branchProvider.GetCurrentBranchId();
        var soldItems = await _uow.SalesInvoices.GetQueryable()
            .Include(i => i.Items)
            .Where(i => !i.IsDeleted && i.Status != InvoiceStatus.Cancelled && !i.IsHold &&
                        i.BranchId == branchId &&
                        i.InvoiceDate >= since)
            .SelectMany(i => i.Items)
            .GroupBy(item => item.ProductVariantId)
            .Select(g => new { VariantId = g.Key, TotalSold = g.Sum(x => x.Quantity) })
            .ToListAsync();

        var velocityMap = soldItems.ToDictionary(
            x => x.VariantId,
            x => (decimal)x.TotalSold / settings.AnalysisPeriodDays);

       
        var variants = await _uow.ProductVariants.GetAllWithDetailsAsync();
        var suppliers = await _uow.PurchaseOrderItems.GetQueryable()
            .Include(poi => poi.PurchaseOrder).ThenInclude(po => po.Supplier)
            .Where(poi => !poi.IsDeleted)
            .GroupBy(poi => poi.ProductVariantId)
            .Select(g => new { VariantId = g.Key, LastSupplier = g.OrderByDescending(x => x.PurchaseOrder.OrderDate).First().PurchaseOrder.Supplier })
            .ToListAsync();

        var suggestions = new List<ReorderSuggestionDto>();

        foreach (var variant in variants.Where(v => v.IsActive && v.Product.IsActive && v.Product.BranchId == branchId))
        {
            if (!velocityMap.TryGetValue(variant.Id, out var dailyVelocity)) continue;
            if (dailyVelocity < settings.MinDailyVelocity) continue;

            var currentStock = variant.Stock?.Quantity ?? 0;   // decimal
            var daysUntilStockout = dailyVelocity > 0 ? (int)Math.Floor(currentStock / dailyVelocity) : 999;

            var supplierInfo = suppliers.FirstOrDefault(s => s.VariantId == variant.Id)?.LastSupplier;
            var leadTime = supplierInfo?.AverageLeadTimeDays ?? settings.DefaultLeadTimeDays;

            var triggerThreshold = leadTime + settings.SafetyStockDays;
            if (daysUntilStockout > triggerThreshold) continue;

            var coverageDays = leadTime + settings.SafetyStockDays + settings.AnalysisPeriodDays / 4;
            var suggestedQty = (int)Math.Ceiling(dailyVelocity * coverageDays) - (int)currentStock;  
            if (suggestedQty <= 0) continue;

            var urgency = daysUntilStockout <= leadTime / 2 ? "Critical"
                        : daysUntilStockout <= leadTime ? "High"
                        : daysUntilStockout <= triggerThreshold ? "Medium"
                        : "Low";

            var costPrice = variant.CostPriceOverride ?? variant.Product.CostPrice;

            suggestions.Add(new ReorderSuggestionDto
            {
                ProductVariantId = variant.Id,
                ProductName = variant.Product.Name,
                SizeName = variant.Size.Name,
                ColorName = variant.Color.Name,
                Barcode = variant.Barcode,
                CurrentStock = (int)currentStock,  
                DailyVelocity = Math.Round(dailyVelocity, 2),
                DaysUntilStockout = daysUntilStockout,
                EstimatedStockoutDate = DateTime.Today.AddDays(daysUntilStockout),
                SuggestedReorderQty = suggestedQty,
                Urgency = urgency,
                PreferredSupplierId = supplierInfo?.Id,
                PreferredSupplierName = supplierInfo?.CompanyName,
                EstimatedCost = suggestedQty * costPrice
            });
        }


        return suggestions.OrderBy(s => s.DaysUntilStockout).ToList();
    }


    public async Task<ReorderSummaryDto> GetSummaryAsync()
    {
        var suggestions = await GetSuggestionsAsync();
        return new ReorderSummaryDto
        {
            CriticalCount = suggestions.Count(s => s.Urgency == "Critical"),
            HighCount = suggestions.Count(s => s.Urgency == "High"),
            MediumCount = suggestions.Count(s => s.Urgency == "Medium"),
            TotalEstimatedCost = suggestions.Sum(s => s.EstimatedCost)
        };
    }


    public async Task<ServiceResult<int>> GeneratePurchaseOrderAsync(GeneratePOFromSuggestionsDto dto, int userId)
    {
        var branchId = _branchProvider.GetCurrentBranchId();
        var supplier = await _uow.Suppliers.GetByIdAsync(dto.SupplierId);
        if (supplier == null) return ServiceResult<int>.Fail("Supplier not found.");

        if (!dto.Items.Any()) return ServiceResult<int>.Fail("কোনো item সিলেক্ট করা হয়নি।");

        var po = new PurchaseOrder
        {
            PONumber = $"PO-{DateTime.Now:yyyyMMddHHmmss}",
            SupplierId = dto.SupplierId,
            OrderDate = DateTime.UtcNow,
            Status = PurchaseOrderStatus.Draft,
            Notes = "Smart Reorder Suggestion থেকে স্বয়ংক্রিয়ভাবে তৈরি",
            BranchId = branchId,
            CreatedBy = userId
        };
        await _uow.PurchaseOrders.AddAsync(po);
        await _uow.SaveChangesAsync();

        decimal totalAmount = 0;
        foreach (var item in dto.Items)
        {
            var variant = await _uow.ProductVariants.GetByIdAsync(item.ProductVariantId);
            if (variant == null) continue;

            var costPrice = variant.CostPriceOverride ?? variant.Product.CostPrice;
            var lineTotal = costPrice * item.Quantity;
            totalAmount += lineTotal;

            await _uow.PurchaseOrderItems.AddAsync(new PurchaseOrderItem
            {
                PurchaseOrderId = po.Id,
                ProductVariantId = item.ProductVariantId,
                OrderedQuantity = item.Quantity,      
                ReceivedQuantity = 0,                  
                UnitCost = costPrice,
                TotalCost = lineTotal,
                CreatedBy = userId
            });
        }

        po.TotalAmount = totalAmount;
        _uow.PurchaseOrders.Update(po);
        await _uow.SaveChangesAsync();

        return ServiceResult<int>.Ok(po.Id, $"Draft Purchase Order {po.PONumber} তৈরি হয়েছে — অনুমোদনের জন্য review করুন।");
    }
}