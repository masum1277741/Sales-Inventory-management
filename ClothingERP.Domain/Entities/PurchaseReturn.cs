namespace ClothingERP.Domain.Entities;

public class PurchaseReturn : BaseEntity
{
    public string ReturnNumber { get; set; } = string.Empty;
    public int PurchaseOrderId { get; set; }
    public int SupplierId { get; set; }
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
    public string? Reason { get; set; }
    public decimal TotalAmount { get; set; }

    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
    public virtual Supplier Supplier { get; set; } = null!;
    public virtual ICollection<PurchaseReturnItem> Items { get; set; } = new List<PurchaseReturnItem>();
}