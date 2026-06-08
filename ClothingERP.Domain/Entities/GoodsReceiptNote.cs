namespace ClothingERP.Domain.Entities;

public class GoodsReceiptNote : BaseEntity
{
    public string GRNNumber { get; set; } = string.Empty;
    public int PurchaseOrderId { get; set; }
    public int SupplierId { get; set; }
    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
    public string? DeliveryChallan { get; set; }
    public string? Notes { get; set; }

    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    public virtual Supplier Supplier { get; set; } = null!;
    public virtual ICollection<GoodsReceiptNoteItem> Items { get; set; } = new List<GoodsReceiptNoteItem>();
}