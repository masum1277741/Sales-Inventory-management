namespace ClothingERP.Application.DTOs;

public class ForecastSettingsDto
{
    public int AnalysisPeriodDays { get; set; }
    public int ForecastHorizonDays { get; set; }
    public decimal Alpha { get; set; }
    public decimal Beta { get; set; }
    public decimal Gamma { get; set; }
    public int MinDataPointsRequired { get; set; }
}

public class UpdateForecastSettingsDto
{
    [Required, Range(30, 365)] public int AnalysisPeriodDays { get; set; }
    [Required, Range(7, 90)] public int ForecastHorizonDays { get; set; }
    [Required, Range(0.01, 1)] public decimal Alpha { get; set; }
    [Required, Range(0.01, 1)] public decimal Beta { get; set; }
    [Required, Range(0.01, 1)] public decimal Gamma { get; set; }
    [Required, Range(7, 60)] public int MinDataPointsRequired { get; set; }
}


public class DemandForecastDto
{
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;

    public bool HasSufficientData { get; set; }
    public string TrendDirection { get; set; } = "Stable";   // Growing | Declining | Stable | Unknown
    public decimal TrendPercentage { get; set; }             
    public string ConfidenceLevel { get; set; } = "Low";       // High | Medium | Low
    public decimal ConfidenceScore { get; set; }                // 0-100

    public decimal PredictedDemandNext7Days { get; set; }
    public decimal PredictedDemandNext14Days { get; set; }
    public decimal PredictedDemandNext30Days { get; set; }

    public int CurrentStock { get; set; }
    public int RecommendedStockLevel { get; set; }  


    public List<ForecastPointDto> HistoricalSeries { get; set; } = new();
    public List<ForecastPointDto> ForecastSeries { get; set; } = new();
}

public class ForecastPointDto
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
}

public class TopMoverDto
{
    public int ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SizeName { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public decimal TrendPercentage { get; set; }
    public decimal PredictedDemandNext30Days { get; set; }
}

public class ForecastSummaryDto
{
    public List<TopMoverDto> TopGrowing { get; set; } = new();
    public List<TopMoverDto> TopDeclining { get; set; } = new();
    public int TotalProductsAnalyzed { get; set; }
    public int SufficientDataCount { get; set; }
}