namespace ClothingERP.Domain.Entities;

public class Supplier : BaseEntity
{
    public string CompanyName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public int BranchId { get; set; }
    public decimal CurrentBalance { get; set; } // positive = we owe them
    public bool IsActive { get; set; } = true;
    public int? AverageLeadTimeDays { get; set; }
    public virtual Branch Branch { get; set; } = null!;
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    public virtual ICollection<SupplierLedger> LedgerEntries { get; set; } = new List<SupplierLedger>();
}
