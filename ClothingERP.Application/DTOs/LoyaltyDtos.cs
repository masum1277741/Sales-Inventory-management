namespace ClothingERP.Application.DTOs;

public class LoyaltySettingsDto
{
    public int Id { get; set; }
    public bool IsEnabled { get; set; }
    public decimal PointsPerDollarSpent { get; set; }
    public decimal PointValueInDollars { get; set; }
    public int MinPointsToRedeem { get; set; }
    public int BirthdayBonusPoints { get; set; }
}

public class UpdateLoyaltySettingsDto
{
    [Required] public bool IsEnabled { get; set; }
    [Required, Range(0, 100)] public decimal PointsPerDollarSpent { get; set; }
    [Required, Range(0, 1)] public decimal PointValueInDollars { get; set; }
    [Required, Range(0, 100000)] public int MinPointsToRedeem { get; set; }
    [Required, Range(0, 10000)] public int BirthdayBonusPoints { get; set; }
}

public class LoyaltyTransactionDto
{
    public int Id { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public int Points { get; set; }
    public string? Description { get; set; }
    public int? SalesInvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime TransactionDate { get; set; }
    public int BalanceAfter { get; set; }
}

public class CustomerLoyaltyDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int CurrentPoints { get; set; }
    public decimal RedeemableValueUSD { get; set; } // current points এর ডলার মূল্য
    public bool CanRedeem { get; set; }
    public List<LoyaltyTransactionDto> RecentHistory { get; set; } = new();
}

// ── Redeem preview (POS এ ব্যবহার হবে) ─────────────────────────────────
public class RedeemPreviewDto
{
    public int PointsToRedeem { get; set; }
    public decimal DiscountValue { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}