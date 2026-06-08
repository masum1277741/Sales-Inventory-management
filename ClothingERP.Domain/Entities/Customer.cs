namespace ClothingERP.Domain.Entities;

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? NIDNumber { get; set; }
    public string? ProfileImage { get; set; }
    public int CustomerGroupId { get; set; }
    public decimal LoyaltyPoints { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
    public decimal CurrentBalance { get; set; } // positive = customer owes us
    public bool IsActive { get; set; } = true;

    public virtual CustomerGroup CustomerGroup { get; set; } = null!;
    public virtual ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
    public virtual ICollection<CustomerLedger> LedgerEntries { get; set; } = new List<CustomerLedger>();
}