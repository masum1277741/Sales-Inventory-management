namespace ClothingERP.Domain.Entities;

public class SalesInvoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Confirmed;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public bool IsCredit { get; set; }
    public bool IsHold { get; set; }
    public string? Notes { get; set; }
    public decimal? TotalAmountBDT { get; set; }
    public decimal? TotalAmountMVR { get; set; }
    public decimal? ExchangeRateBDT { get; set; }
    public decimal? ExchangeRateMVR { get; set; }

    [NotMapped]
    public decimal DueAmount => TotalAmount - PaidAmount;
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
    public virtual ICollection<SalesPayment> Payments { get; set; } = new List<SalesPayment>();
}