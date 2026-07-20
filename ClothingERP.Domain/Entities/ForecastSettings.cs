namespace ClothingERP.Domain.Entities;

public class ForecastSettings : BaseEntity
{
    public int AnalysisPeriodDays { get; set; } = 90;
    public int ForecastHorizonDays { get; set; } = 30;
    public decimal Alpha { get; set; } = 0.3m;
    public decimal Beta { get; set; } = 0.1m;
    public decimal Gamma { get; set; } = 0.3m;
    public int MinDataPointsRequired { get; set; } = 14;
}