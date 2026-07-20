namespace ClothingERP.Domain.Entities;

public class ReorderSettings : BaseEntity
{
    public int AnalysisPeriodDays { get; set; } = 30;  
    public int DefaultLeadTimeDays { get; set; } = 7;  
    public int SafetyStockDays { get; set; } = 5;   
    public decimal MinDailyVelocity { get; set; } = 0.1m; 
}