namespace ClothingERP.Application.DTOs;

public class BulkActionResultDto
{
    public bool Success { get; set; }
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}

// ── Bulk Price Update (Products) ─────────────────────────────────────────
public class BulkPriceUpdateDto
{
    [Required, MinLength(1)] public List<int> ProductIds { get; set; } = new();

    [Required] public string PriceField { get; set; } = "RetailPrice"; // RetailPrice | CostPrice | Both
    [Required] public string Mode { get; set; } = "Percent";     // Percent | Fixed
    [Required] public string Direction { get; set; } = "Increase";    // Increase | Decrease
    [Required, Range(0.01, 100000)] public decimal Value { get; set; }
}

// ── Bulk Status Toggle (generic — Product/Customer/Supplier সবার জন্য) ────
public class BulkStatusUpdateDto
{
    [Required, MinLength(1)] public List<int> Ids { get; set; } = new();
    [Required] public bool IsActive { get; set; }
}

// ── Bulk Delete (generic) ──────────────────────────────────────────────
public class BulkDeleteDto
{
    [Required, MinLength(1)] public List<int> Ids { get; set; } = new();
}

// ── Bulk Category/Brand Reassign (Products) ──────────────────────────────
public class BulkCategoryUpdateDto
{
    [Required, MinLength(1)] public List<int> ProductIds { get; set; } = new();
    public int? CategoryId { get; set; }
    public int? SubCategoryId { get; set; }
    public int? BrandId { get; set; }
}