namespace ClothingERP.Domain.Entities;

public class CustomerLedger : BaseEntity
{
    public int CustomerId { get; set; }
    public LedgerEntryType EntryType { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Description { get; set; }
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;

    public virtual Customer Customer { get; set; } = null!;
}