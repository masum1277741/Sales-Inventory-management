namespace ClothingERP.Domain.Entities;

public class AccountTransaction : BaseEntity
{
    public string TransactionNumber { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public AccountCategory Category { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public PaymentMethod PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public int? BranchId { get; set; }  
    public Branch? Branch { get; set; }
}