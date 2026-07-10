namespace ClothingERP.Domain.Entities;

public class ProductBundle : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BundlePrice { get; set; }     // USD — বিশেষ combo দাম
    public string? ImagePath { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }      // সময়-সীমিত অফার (optional)
    public DateTime? EndDate { get; set; }

    public ICollection<ProductBundleItem> Items { get; set; } = new List<ProductBundleItem>();
}