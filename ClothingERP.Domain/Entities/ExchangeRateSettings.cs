namespace ClothingERP.Domain.Entities;

public class ExchangeRateSettings : BaseEntity
{
    public bool AutoUpdateEnabled { get; set; } = true;
    public int UpdateIntervalHours { get; set; } = 24;   
    public DateTime? LastAutoUpdateAt { get; set; }
    public DateTime? LastFailedAttemptAt { get; set; }
    public string? LastErrorMessage { get; set; }
}