using System.Collections;

namespace ClothingERP.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public int ProductId { get; set; }
    public int SizeId { get; set; }
    public int ColorId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string? QRCode { get; set; }
    public decimal? CostPriceOverride { get; set; }
    public decimal? RetailPriceOverride { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual Product Product { get; set; } = null!;
    public virtual Size Size { get; set; } = null!;
    public virtual Color Color { get; set; } = null!;
    public virtual Stock? Stock { get; set; }
}