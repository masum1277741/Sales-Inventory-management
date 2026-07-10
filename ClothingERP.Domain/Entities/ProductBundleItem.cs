namespace ClothingERP.Domain.Entities;

public class ProductBundleItem : BaseEntity
{
    public int ProductBundleId { get; set; }
    public int ProductVariantId { get; set; }
    public int Quantity { get; set; } = 1;

    public ProductBundle ProductBundle { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;
}