namespace ClothingERP.Domain.Entities;

public class SalesReturn : BaseEntity
{
    public string ReturnNumber { get; set; } = string.Empty;
    public int SalesInvoiceId { get; set; }
    public int? CustomerId { get; set; }
    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
    public ReturnType ReturnType { get; set; }
    public string? Reason { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal RefundAmount { get; set; }
    public RefundMethod RefundMethod { get; set; }

    public virtual SalesInvoice SalesInvoice { get; set; } = null!;
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<SalesReturnItem> Items { get; set; } = new List<SalesReturnItem>();
}