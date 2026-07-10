namespace ClothingERP.Domain.Entities;

public class CommissionTransaction : BaseEntity
{
    public int UserId { get; set; }    // যেই staff এই sale করেছে
    public int SalesInvoiceId { get; set; }
    public decimal SaleAmount { get; set; }
    public decimal CommissionPercent { get; set; }
    public decimal CommissionAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Paid, Reversed
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public DateTime? PaidDate { get; set; }
    public int? PaidBy { get; set; }
    public string? Notes { get; set; }

    public User User { get; set; } = null!;
    public SalesInvoice SalesInvoice { get; set; } = null!;
}