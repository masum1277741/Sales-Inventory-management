namespace ClothingERP.Application.DTOs;

// ── Category ─────────────────────────────────────────────────────────────
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public bool IsActive { get; set; }
    public int SubCategoryCount { get; set; }
}

public class CreateCategoryDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ImagePath { get; set; }
}

// ── SubCategory ───────────────────────────────────────────────────────────
public class SubCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class CreateSubCategoryDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required]
    public int CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
}

// ── Brand ────────────────────────────────────────────────────────────────
public class BrandDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoPath { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
}

public class CreateBrandDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string? LogoPath { get; set; }
}

// ── Size & Color ──────────────────────────────────────────────────────────
public class SizeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSizeDto
{
    [Required, MaxLength(20)]
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class ColorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HexCode { get; set; } = "#000000";
    public bool IsActive { get; set; }
}

public class CreateColorDto
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string HexCode { get; set; } = "#000000";
}

// ── Product ───────────────────────────────────────────────────────────────
public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string SubCategoryName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public decimal RetailPrice { get; set; }
    public int ReorderPoint { get; set; }
    public bool IsActive { get; set; }
    public int VariantCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int SubCategoryId { get; set; }
    public string SubCategoryName { get; set; } = string.Empty;
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public decimal RetailPrice { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal? SpecialPrice { get; set; }
    public decimal TaxRate { get; set; }
    public int ReorderPoint { get; set; }
    public bool IsActive { get; set; }
    public List<ProductVariantDto> Variants { get; set; } = new();
}

public class CreateProductDto
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required]
    public int CategoryId { get; set; }
    [Required]
    public int SubCategoryId { get; set; }
    [Required]
    public int BrandId { get; set; }
    [Required, Range(0, 9999999)]
    public decimal CostPrice { get; set; }
    [Required, Range(0, 9999999)]
    public decimal RetailPrice { get; set; }
    [Range(0, 9999999)]
    public decimal WholesalePrice { get; set; }
    public decimal? SpecialPrice { get; set; }
    public decimal TaxRate { get; set; }
    public int ReorderPoint { get; set; } = 5;
    public bool IsActive { get; set; } = true;
    public string? ImagePath { get; set; }
    public List<CreateProductVariantDto> Variants { get; set; } = new();
}

public class UpdateProductDto : CreateProductDto { }

// ── Variant ───────────────────────────────────────────────────────────────
public class ProductVariantDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public int SizeId { get; set; }
    public string SizeName { get; set; } = string.Empty;
    public int ColorId { get; set; }
    public string ColorName { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal? CostPriceOverride { get; set; }
    public decimal? RetailPriceOverride { get; set; }
    public decimal EffectiveCostPrice { get; set; }
    public decimal EffectiveRetailPrice { get; set; }
    public decimal StockQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class CreateProductVariantDto
{
    [Required]
    public int SizeId { get; set; }
    [Required]
    public int ColorId { get; set; }
    public decimal? CostPriceOverride { get; set; }
    public decimal? RetailPriceOverride { get; set; }
}