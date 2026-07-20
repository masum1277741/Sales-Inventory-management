namespace ClothingERP.Domain.Entities;

public class ExchangeRateSnapshot : BaseEntity
{
    public string BaseCurrency { get; set; } = "USD";
    public string TargetCurrency { get; set; } = string.Empty;  // "BDT" | "MVR"
    public decimal Rate { get; set; }
    public string Source { get; set; } = "API";          // API | Manual
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;            //  active snapshot = current rate
}