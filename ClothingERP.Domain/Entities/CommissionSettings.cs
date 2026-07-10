namespace ClothingERP.Domain.Entities;

public class CommissionSettings : BaseEntity
{
    public bool IsEnabled { get; set; } = true;
    public decimal DefaultCommissionPercent { get; set; } = 2m;   // staff এর জন্য override না থাকলে এই rate
    public decimal MinSaleAmountForCommission { get; set; } = 0m; // এর কম sale হলে commission নাই
    public bool ExcludeReturnsFromCommission { get; set; } = true;
}