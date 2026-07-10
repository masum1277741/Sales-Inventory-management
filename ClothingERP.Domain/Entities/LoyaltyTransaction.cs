namespace ClothingERP.Domain.Entities;

public class LoyaltyTransaction : BaseEntity
{
    public int CustomerId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Earned, Redeemed, Bonus, Expired, Adjustment
    public int Points { get; set; }   // Earned/Bonus = positive, Redeemed = negative
    public string? Description { get; set; }
    public int? SalesInvoiceId { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public int BalanceAfter { get; set; }   // এই transaction পরে balance কত হলো

    public Customer Customer { get; set; } = null!;
}