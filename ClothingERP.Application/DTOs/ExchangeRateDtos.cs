namespace ClothingERP.Application.DTOs;

public class CurrentRatesDto
{
    public decimal UsdToBdt { get; set; }
    public decimal UsdToMvr { get; set; }
    public DateTime LastUpdated { get; set; }
    public string Source { get; set; } = string.Empty;
    public bool IsStale { get; set; }  
}

public class ExchangeRateHistoryDto
{
    public int Id { get; set; }
    public string TargetCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime FetchedAt { get; set; }
}

public class ExchangeRateSettingsDto
{
    public bool AutoUpdateEnabled { get; set; }
    public int UpdateIntervalHours { get; set; }
    public DateTime? LastAutoUpdateAt { get; set; }
    public DateTime? LastFailedAttemptAt { get; set; }
    public string? LastErrorMessage { get; set; }
}

public class UpdateExchangeRateSettingsDto
{
    [Required] public bool AutoUpdateEnabled { get; set; }
    [Required, Range(1, 168)] public int UpdateIntervalHours { get; set; }  
}

public class ManualRateOverrideDto
{
    [Required] public string TargetCurrency { get; set; } = string.Empty;  // "BDT" | "MVR"
    [Required, Range(0.0001, 100000)] public decimal Rate { get; set; }
}

public class RefreshResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public CurrentRatesDto? Rates { get; set; }
}