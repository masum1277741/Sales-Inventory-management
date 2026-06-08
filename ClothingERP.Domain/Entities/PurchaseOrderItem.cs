namespace ClothingERP.Domain.Entities;

public class PurchaseOrderItem : BaseEntity
{
    public int PurchaseOrderId { get; set; }
    public int ProductVariantId { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalCost { get; set; }

    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}