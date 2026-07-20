namespace ClothingERP.Domain.Entities;

public class OnlineOrderItem : BaseEntity
{
    public int OnlineOrderId { get; set; }
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;   // snapshot — পরে product edit হলেও order history ঠিক থাকবে
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPriceUSD { get; set; }
    public decimal LineTotalUSD { get; set; }

    public OnlineOrder OnlineOrder { get; set; } = null!;
    public ProductVariant ProductVariant { get; set; } = null!;
}