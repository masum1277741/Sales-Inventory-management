namespace ClothingERP.Application.DTOs;

public class ProductBundleListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BundlePrice { get; set; }
    public decimal RegularPrice { get; set; }   // আলাদাভাবে কিনলে যা হতো
    public decimal SavingsAmount { get; set; }
    public decimal SavingsPercent { get; set; }
    public int ItemCount { get; set; }
    public int AvailableStock { get; set; }
    public bool IsActive { get; set; }
}

public class ProductBundleDto : ProductBundleListDto
{
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<ProductBundleItemDto> Items { get; set; } = new();
}

public class ProductBundleItemDto
{
    public int Id { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }   // regular retail price
    public int Quantity { get; set; }
    public int AvailableStock { get; set; }
}

public class CreateProductBundleDto
{
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required, Range(0.01, 999999)] public decimal BundlePrice { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    [MinLength(2, ErrorMessage = "একটা bundle এ কমপক্ষে ২টা item থাকতে হবে")]
    public List<CreateBundleItemDto> Items { get; set; } = new();
}

public class CreateBundleItemDto
{
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; } = 1;
}

// ── POS Search এর জন্য ───────────────────────────────────────────────────
public class BundleSearchDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BundlePrice { get; set; }
    public decimal RegularPrice { get; set; }
    public decimal SavingsAmount { get; set; }
    public int AvailableStock { get; set; }
    public List<string> ItemsSummary { get; set; } = new(); // ["M·Black x1", "Belt x1"]
}