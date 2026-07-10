namespace ClothingERP.Domain.Entities;

public class LoyaltySettings : BaseEntity
{
    public bool IsEnabled { get; set; } = true;
    public decimal PointsPerDollarSpent { get; set; } = 1m;     // ১ ডলারে কত পয়েন্ট
    public decimal PointValueInDollars { get; set; } = 0.01m;  // ১ পয়েন্ট = কত ডলার
    public int MinPointsToRedeem { get; set; } = 100;    // রিডিম করার জন্য সর্বনিম্ন পয়েন্ট
    public int BirthdayBonusPoints { get; set; } = 50;     // জন্মদিনে বোনাস
}