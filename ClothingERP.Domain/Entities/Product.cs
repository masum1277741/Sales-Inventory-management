namespace ClothingERP.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public int CategoryId { get; set; }
    public int SubCategoryId { get; set; }
    public int BrandId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal RetailPrice { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal? SpecialPrice { get; set; }
    public decimal TaxRate { get; set; }
    public int ReorderPoint { get; set; } = 5;
    public bool IsActive { get; set; } = true;

    public virtual Category Category { get; set; } = null!;
    public virtual SubCategory SubCategory { get; set; } = null!;
    public virtual Brand Brand { get; set; } = null!;
    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}