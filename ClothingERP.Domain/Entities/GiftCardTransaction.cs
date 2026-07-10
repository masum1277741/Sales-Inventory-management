namespace ClothingERP.Domain.Entities;

public class GiftCardTransaction : BaseEntity
{
    public int GiftCardId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Issued, Redeemed, Adjustment, Expired, Cancelled
    public decimal Amount { get; set; }   // Issued = positive, Redeemed = negative
    public decimal BalanceAfter { get; set; }
    public int? SalesInvoiceId { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public GiftCard GiftCard { get; set; } = null!;
}