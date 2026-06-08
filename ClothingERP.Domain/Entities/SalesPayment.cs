namespace ClothingERP.Domain.Entities;

public class SalesPayment : BaseEntity
{
    public int SalesInvoiceId { get; set; }
    public int? CustomerId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public virtual SalesInvoice SalesInvoice { get; set; } = null!;
}