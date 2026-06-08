namespace ClothingERP.Domain.Entities;

public class GoodsReceiptNoteItem : BaseEntity
{
    public int GoodsReceiptNoteId { get; set; }
    public int ProductVariantId { get; set; }
    public int PurchaseOrderItemId { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    public virtual GoodsReceiptNote GoodsReceiptNote { get; set; } = null!;
    public virtual ProductVariant ProductVariant { get; set; } = null!;
    public virtual PurchaseOrderItem PurchaseOrderItem { get; set; } = null!;
}