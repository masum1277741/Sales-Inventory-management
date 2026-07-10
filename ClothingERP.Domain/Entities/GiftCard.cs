namespace ClothingERP.Domain.Entities;

public class GiftCard : BaseEntity
{
    public string CardCode { get; set; } = string.Empty;  // GC-XXXX-XXXX-XXXX
    public decimal InitialValue { get; set; }
    public decimal CurrentBalance { get; set; }
    public int? IssuedToCustomerId { get; set; }
    public string? RecipientName { get; set; }   // anonymous gift হলেও নাম রাখার জন্য
    public string? RecipientPhone { get; set; }
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Depleted, Expired, Cancelled
    public bool IsStoreCredit { get; set; } = false;     // true হলে return/refund থেকে issued
    public int? SourceReturnId { get; set; }              // store credit হলে কোন return থেকে এলো
    public string? Notes { get; set; }

    public Customer? IssuedToCustomer { get; set; }
    public ICollection<GiftCardTransaction> Transactions { get; set; } = new List<GiftCardTransaction>();
}