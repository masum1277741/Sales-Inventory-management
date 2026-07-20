namespace ClothingERP.Application.Services;

public class DemandForecastService : IDemandForecastService
{
    private readonly IUnitOfWork _uow;
    private const int SeasonLength = 7;   

    public DemandForecastService(IUnitOfWork uow) => _uow = uow;

    // ── Settings ──────────────────────────────────────────────────────────
    public async Task<ForecastSettingsDto> GetSettingsAsync()
    {
        var settings = (await _uow.ForecastSettings.GetAllAsync()).FirstOrDefault();
        if (settings == null)
        {
            settings = new ForecastSettings();
            await _uow.ForecastSettings.AddAsync(settings);
            await _uow.SaveChangesAsync();
        }

        return new ForecastSettingsDto
        {
            AnalysisPeriodDays = settings.AnalysisPeriodDays,
            ForecastHorizonDays = settings.ForecastHorizonDays,
            Alpha = settings.Alpha,
            Beta = settings.Beta,
            Gamma = settings.Gamma,
            MinDataPointsRequired = settings.MinDataPointsRequired
        };
    }

    public async Task<ServiceResult> UpdateSettingsAsync(UpdateForecastSettingsDto dto, int userId)
    {
        var settings = (await _uow.ForecastSettings.GetAllAsync()).FirstOrDefault();
        if (settings == null) { settings = new ForecastSettings(); await _uow.ForecastSettings.AddAsync(settings); }

        settings.AnalysisPeriodDays = dto.AnalysisPeriodDays;
        settings.ForecastHorizonDays = dto.ForecastHorizonDays;
        settings.Alpha = dto.Alpha;
        settings.Beta = dto.Beta;
        settings.Gamma = dto.Gamma;
        settings.MinDataPointsRequired = dto.MinDataPointsRequired;
        settings.UpdatedBy = userId;
        settings.UpdatedAt = DateTime.UtcNow;

        _uow.ForecastSettings.Update(settings);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Forecast settings updated successfully.");
    }


    private async Task<double[]> BuildDailySeriesAsync(int variantId, int periodDays)
    {
        var since = DateTime.UtcNow.Date.AddDays(-periodDays);

        var dailySales = await _uow.SalesInvoices.GetQueryable()
            .Include(i => i.Items)
            .Where(i => !i.IsDeleted && i.Status != InvoiceStatus.Cancelled && !i.IsHold &&
                        i.InvoiceDate >= since)
            .SelectMany(i => i.Items.Where(it => it.ProductVariantId == variantId)
                                     .Select(it => new { i.InvoiceDate.Date, it.Quantity }))
            .ToListAsync();

        var grouped = dailySales
            .GroupBy(x => x.Date)
            .ToDictionary(g => g.Key, g => (double)g.Sum(x => x.Quantity));


        var series = new double[periodDays];
        for (int i = 0; i < periodDays; i++)
        {
            var date = since.AddDays(i);
            series[i] = grouped.TryGetValue(date, out var qty) ? qty : 0;
        }
        return series;
    }


    private class HoltWintersResult
    {
        public double[] Fitted = Array.Empty<double>();  
        public double[] Forecast = Array.Empty<double>();  
        public double FinalLevel, FinalTrend;
        public double[] SeasonalIndices = Array.Empty<double>();
    }

    private HoltWintersResult RunHoltWinters(double[] series, double alpha, double beta, double gamma, int horizon)
    {
        int n = series.Length;
        int m = SeasonLength;


        var season1Avg = series.Take(m).Average();
        var season2Avg = series.Skip(m).Take(m).Average();

        double level = season1Avg;
        double trend = (season2Avg - season1Avg) / m;


        var seasonal = new double[m];
        var seasonCounts = new int[m];
        for (int i = 0; i < n; i++)
        {
            seasonal[i % m] += series[i] - season1Avg;
            seasonCounts[i % m]++;
        }
        for (int i = 0; i < m; i++)
            seasonal[i] = seasonCounts[i] > 0 ? seasonal[i] / seasonCounts[i] : 0;

        var fitted = new double[n];
        var levels = new double[n];
        var trends = new double[n];

        // ── Recursive Smoothing ───────────────────────────────────────────────
        for (int t = 0; t < n; t++)
        {
            int seasonIdx = t % m;
            double prevLevel = t == 0 ? level : levels[t - 1];
            double prevTrend = t == 0 ? trend : trends[t - 1];
            double prevSeasonal = seasonal[seasonIdx];

            fitted[t] = prevLevel + prevTrend + prevSeasonal;

            double newLevel = alpha * (series[t] - prevSeasonal) + (1 - alpha) * (prevLevel + prevTrend);
            double newTrend = beta * (newLevel - prevLevel) + (1 - beta) * prevTrend;
            double newSeasonal = gamma * (series[t] - newLevel) + (1 - gamma) * prevSeasonal;

            levels[t] = newLevel;
            trends[t] = newTrend;
            seasonal[seasonIdx] = newSeasonal;
        }

        var forecast = new double[horizon];
        var finalLevel = levels[n - 1];
        var finalTrend = trends[n - 1];

        for (int h = 1; h <= horizon; h++)
        {
            int seasonIdx = (n + h - 1) % m;
            var value = finalLevel + h * finalTrend + seasonal[seasonIdx];
            forecast[h - 1] = Math.Max(0, value);  
        }

        return new HoltWintersResult
        {
            Fitted = fitted,
            Forecast = forecast,
            FinalLevel = finalLevel,
            FinalTrend = finalTrend,
            SeasonalIndices = seasonal
        };
    }


    private decimal CalculateConfidence(double[] fitted, double[] actual, int testWindow)
    {
        int n = actual.Length;
        if (n < testWindow * 2) return 30;  

        var errors = new List<double>();
        for (int i = n - testWindow; i < n; i++)
        {
            if (actual[i] <= 0.5) continue; 
            var error = Math.Abs(actual[i] - fitted[i]) / actual[i];
            errors.Add(error);
        }

        if (!errors.Any()) return 50; 

        var mape = errors.Average() * 100;
        var confidence = Math.Max(0, Math.Min(100, 100 - mape));
        return (decimal)Math.Round(confidence, 1);
    }


    public async Task<DemandForecastDto> ForecastForVariantAsync(int variantId)
    {
        var settings = await GetSettingsAsync();
        var variant = await _uow.ProductVariants.GetByIdAsync(variantId);

        var result = new DemandForecastDto
        {
            ProductVariantId = variantId,
            ProductName = variant?.Product?.Name ?? "Unknown",
            SizeName = variant?.Size?.Name ?? "",
            ColorName = variant?.Color?.Name ?? "",
            CurrentStock = (int)((await _uow.Stocks.GetByVariantIdAsync(variantId))?.Quantity ?? 0)
        };

        var series = await BuildDailySeriesAsync(variantId, settings.AnalysisPeriodDays);
        var nonZeroDays = series.Count(v => v > 0);

 
        if (nonZeroDays < settings.MinDataPointsRequired || series.Length < SeasonLength * 2)
        {
            result.HasSufficientData = false;
            result.TrendDirection = "Unknown";
            result.ConfidenceLevel = "Low";
            return result;
        }

        result.HasSufficientData = true;

        var hw = RunHoltWinters(series, (double)settings.Alpha, (double)settings.Beta, (double)settings.Gamma, settings.ForecastHorizonDays);


        var confidence = CalculateConfidence(hw.Fitted, series, testWindow: Math.Min(14, series.Length / 4));
        result.ConfidenceScore = confidence;
        result.ConfidenceLevel = confidence >= 75 ? "High" : confidence >= 50 ? "Medium" : "Low";

        var firstWeekAvg = series.Take(SeasonLength).Average();
        var lastWeekAvg = series.Skip(series.Length - SeasonLength).Average();
        decimal trendPct = 0;
        if (firstWeekAvg > 0.1)
            trendPct = (decimal)((lastWeekAvg - firstWeekAvg) / firstWeekAvg * 100);

        result.TrendPercentage = Math.Round(trendPct, 1);
        result.TrendDirection = trendPct > 8 ? "Growing" : trendPct < -8 ? "Declining" : "Stable";

 
        result.PredictedDemandNext7Days = (decimal)hw.Forecast.Take(Math.Min(7, hw.Forecast.Length)).Sum();
        result.PredictedDemandNext14Days = (decimal)hw.Forecast.Take(Math.Min(14, hw.Forecast.Length)).Sum();
        result.PredictedDemandNext30Days = (decimal)hw.Forecast.Take(Math.Min(30, hw.Forecast.Length)).Sum();

        result.RecommendedStockLevel = (int)Math.Ceiling(result.PredictedDemandNext30Days);

        // ── Chart Data ────────────────────────────────────────────────────────
        var startDate = DateTime.UtcNow.Date.AddDays(-settings.AnalysisPeriodDays);
        result.HistoricalSeries = series.Select((v, i) => new ForecastPointDto
        {
            Date = startDate.AddDays(i),
            Value = (decimal)v
        }).ToList();

        result.ForecastSeries = hw.Forecast.Select((v, i) => new ForecastPointDto
        {
            Date = DateTime.UtcNow.Date.AddDays(i + 1),
            Value = (decimal)Math.Round(v, 2)
        }).ToList();

        return result;
    }


    public async Task<ForecastSummaryDto> GetSummaryAsync(int topCount = 10)
    {
        var settings = await GetSettingsAsync();


        var since = DateTime.UtcNow.Date.AddDays(-settings.AnalysisPeriodDays);
        var activeVariantIds = await _uow.SalesInvoices.GetQueryable()
            .Include(i => i.Items)
            .Where(i => !i.IsDeleted && i.InvoiceDate >= since && i.Status != InvoiceStatus.Cancelled)
            .SelectMany(i => i.Items.Select(it => it.ProductVariantId))
            .Distinct()
            .ToListAsync();

        var allMovers = new List<TopMoverDto>();
        int sufficientCount = 0;

        foreach (var variantId in activeVariantIds)
        {
            var forecast = await ForecastForVariantAsync(variantId);
            if (!forecast.HasSufficientData) continue;
            sufficientCount++;

            allMovers.Add(new TopMoverDto
            {
                ProductVariantId = variantId,
                ProductName = forecast.ProductName,
                SizeName = forecast.SizeName,
                ColorName = forecast.ColorName,
                TrendPercentage = forecast.TrendPercentage,
                PredictedDemandNext30Days = forecast.PredictedDemandNext30Days
            });
        }

        return new ForecastSummaryDto
        {
            TopGrowing = allMovers.OrderByDescending(m => m.TrendPercentage).Take(topCount).ToList(),
            TopDeclining = allMovers.OrderBy(m => m.TrendPercentage).Take(topCount).ToList(),
            TotalProductsAnalyzed = activeVariantIds.Count,
            SufficientDataCount = sufficientCount
        };
    }

    public async Task<List<TopMoverDto>> SearchForecastsAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2) return new();

        var variants = await _uow.ProductVariants.GetAllWithDetailsAsync();
        var matches = variants
            .Where(v => v.IsActive && v.Product.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList();

        var results = new List<TopMoverDto>();
        foreach (var v in matches)
        {
            var forecast = await ForecastForVariantAsync(v.Id);
            results.Add(new TopMoverDto
            {
                ProductVariantId = v.Id,
                ProductName = forecast.ProductName,
                SizeName = forecast.SizeName,
                ColorName = forecast.ColorName,
                TrendPercentage = forecast.TrendPercentage,
                PredictedDemandNext30Days = forecast.PredictedDemandNext30Days
            });
        }
        return results;
    }
}