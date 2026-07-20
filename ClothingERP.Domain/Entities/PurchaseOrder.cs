namespace ClothingERP.Domain.Entities;

public class PurchaseOrder : BaseEntity
{
    public string PONumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string? Notes { get; set; }

    [NotMapped]
    public decimal DueAmount => TotalAmount - PaidAmount;
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public virtual Supplier Supplier { get; set; } = null!;
    public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    public virtual ICollection<GoodsReceiptNote> GRNs { get; set; } = new List<GoodsReceiptNote>();
}