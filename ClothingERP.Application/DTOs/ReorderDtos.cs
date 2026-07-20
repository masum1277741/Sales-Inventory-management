namespace ClothingERP.Application.DTOs;

public class ReorderSuggestionDto
{
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public decimal DailyVelocity { get; set; }  
    public int DaysUntilStockout { get; set; }
    public DateTime EstimatedStockoutDate { get; set; }
    public int SuggestedReorderQty { get; set; }
    public string Urgency { get; set; } = "Low"; // Critical | High | Medium | Low
    public int? PreferredSupplierId { get; set; }
    public string? PreferredSupplierName { get; set; }
    public decimal EstimatedCost { get; set; }   // suggested qty * cost price
}

public class ReorderSummaryDto
{
    public int CriticalCount { get; set; }
    public int HighCount { get; set; }
    public int MediumCount { get; set; }
    public decimal TotalEstimatedCost { get; set; }
}

public class ReorderSettingsDto
{
    public int AnalysisPeriodDays { get; set; }
    public int DefaultLeadTimeDays { get; set; }
    public int SafetyStockDays { get; set; }
    public decimal MinDailyVelocity { get; set; }
}

public class UpdateReorderSettingsDto
{
    [Required, Range(7, 180)] public int AnalysisPeriodDays { get; set; }
    [Required, Range(1, 60)] public int DefaultLeadTimeDays { get; set; }
    [Required, Range(0, 30)] public int SafetyStockDays { get; set; }
    [Required, Range(0, 10)] public decimal MinDailyVelocity { get; set; }
}


public class GeneratePOFromSuggestionsDto
{
    [Required] public int SupplierId { get; set; }
    [Required, MinLength(1)] public List<ReorderPOItemDto> Items { get; set; } = new();
}

public class ReorderPOItemDto
{
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; }
}