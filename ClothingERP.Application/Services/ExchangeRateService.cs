using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace ClothingERP.Application.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly IUnitOfWork _uow;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<ExchangeRateService> _logger;

    // Free, no-API-key external rate provider
    private const string API_URL = "https://open.er-api.com/v6/latest/USD";

 
    private const decimal FALLBACK_BDT = 110.0m;
    private const decimal FALLBACK_MVR = 15.42m;

    public ExchangeRateService(IUnitOfWork uow, IHttpClientFactory httpFactory, ILogger<ExchangeRateService> logger)
        => (_uow, _httpFactory, _logger) = (uow, httpFactory, logger);

  
    public async Task<CurrentRatesDto> GetCurrentRatesAsync()
    {
        var bdtSnap = await GetLatestActiveAsync("BDT");
        var mvrSnap = await GetLatestActiveAsync("MVR");

        var bdt = bdtSnap?.Rate ?? FALLBACK_BDT;
        var mvr = mvrSnap?.Rate ?? FALLBACK_MVR;
        var lastUpdated = new[] { bdtSnap?.FetchedAt, mvrSnap?.FetchedAt }
            .Where(d => d.HasValue).DefaultIfEmpty(DateTime.UtcNow).Max() ?? DateTime.UtcNow;

        var settings = await GetSettingsAsync();
        var staleThreshold = TimeSpan.FromHours(settings.UpdateIntervalHours * 2); 

        return new CurrentRatesDto
        {
            UsdToBdt = bdt,
            UsdToMvr = mvr,
            LastUpdated = lastUpdated,
            Source = bdtSnap?.Source ?? "Default",
            IsStale = (DateTime.UtcNow - lastUpdated) > staleThreshold
        };
    }

    private async Task<ExchangeRateSnapshot?> GetLatestActiveAsync(string currency)
    {
        return await _uow.ExchangeRateSnapshots.GetQueryable()
            .Where(s => !s.IsDeleted && s.IsActive && s.TargetCurrency == currency)
            .OrderByDescending(s => s.FetchedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<RefreshResultDto> RefreshFromApiAsync()
    {
        try
        {
            var client = _httpFactory.CreateClient("ExchangeRateApi");
            var response = await client.GetAsync(API_URL);

            if (!response.IsSuccessStatusCode)
                return await HandleFailureAsync($"API responded with status {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("result", out var resultProp) ||
                resultProp.GetString() != "success")
                return await HandleFailureAsync("API তে invalid response এসেছে।");

            var ratesElement = doc.RootElement.GetProperty("rates");

            decimal? newBdt = ratesElement.TryGetProperty("BDT", out var bdtEl) ? bdtEl.GetDecimal() : null;
            decimal? newMvr = ratesElement.TryGetProperty("MVR", out var mvrEl) ? mvrEl.GetDecimal() : null;

            if (newBdt == null && newMvr == null)
                return await HandleFailureAsync("BDT/MVR rate API response এ পাওয়া যায়নি।");

            if (newBdt.HasValue) await SaveSnapshotAsync("BDT", newBdt.Value, "API");
            if (newMvr.HasValue) await SaveSnapshotAsync("MVR", newMvr.Value, "API");

            await UpdateLastAutoUpdateAsync(success: true, error: null);

            var rates = await GetCurrentRatesAsync();
            return new RefreshResultDto
            {
                Success = true,
                Message = $"Exchange rate সফলভাবে আপডেট হয়েছে — 1 USD = ৳{rates.UsdToBdt:N2} / Rf{rates.UsdToMvr:N2}",
                Rates = rates
            };
        }
        catch (TaskCanceledException)
        {
            return await HandleFailureAsync("API request timeout হয়ে গেছে।");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Exchange rate API refresh failed.");
            return await HandleFailureAsync($"আনাকাঙ্ক্ষিত ত্রুটি: {ex.Message}");
        }
    }

    private async Task<RefreshResultDto> HandleFailureAsync(string error)
    {
        await UpdateLastAutoUpdateAsync(success: false, error: error);
        var current = await GetCurrentRatesAsync();  
        return new RefreshResultDto { Success = false, Message = error, Rates = current };
    }

    private async Task SaveSnapshotAsync(string currency, decimal rate, string source)
    {
        var existingActive = await _uow.ExchangeRateSnapshots.GetQueryable()
            .Where(s => s.TargetCurrency == currency && s.IsActive && !s.IsDeleted)
            .ToListAsync();

        foreach (var old in existingActive)
        {
            old.IsActive = false;
            _uow.ExchangeRateSnapshots.Update(old);
        }

        await _uow.ExchangeRateSnapshots.AddAsync(new ExchangeRateSnapshot
        {
            BaseCurrency = "USD",
            TargetCurrency = currency,
            Rate = rate,
            Source = source,
            FetchedAt = DateTime.UtcNow,
            IsActive = true
        });

        await _uow.SaveChangesAsync();
    }

    // ── Manual Override ───────────────────────────────────────────────────
    public async Task<ServiceResult> SetManualRateAsync(ManualRateOverrideDto dto, int userId)
    {
        var currency = dto.TargetCurrency.ToUpper();
        if (currency != "BDT" && currency != "MVR")
            return ServiceResult.Fail("শুধু BDT অথবা MVR সমর্থিত।");

        await SaveSnapshotAsync(currency, dto.Rate, "Manual");
        return ServiceResult.Ok($"{currency} rate manually সেট করা হলো: {dto.Rate:N4}");
    }

    // ── History (Audit/Transparency) ──────────────────────────────────────
    public async Task<IEnumerable<ExchangeRateHistoryDto>> GetHistoryAsync(int take = 30)
    {
        var snapshots = await _uow.ExchangeRateSnapshots.GetQueryable()
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.FetchedAt)
            .Take(take)
            .ToListAsync();

        return snapshots.Select(s => new ExchangeRateHistoryDto
        {
            Id = s.Id,
            TargetCurrency = s.TargetCurrency,
            Rate = s.Rate,
            Source = s.Source,
            FetchedAt = s.FetchedAt
        });
    }

    // ── Settings ──────────────────────────────────────────────────────────
    public async Task<ExchangeRateSettingsDto> GetSettingsAsync()
    {
        var settings = (await _uow.ExchangeRateSettings.GetAllAsync()).FirstOrDefault();
        if (settings == null)
        {
            settings = new ExchangeRateSettings();
            await _uow.ExchangeRateSettings.AddAsync(settings);
            await _uow.SaveChangesAsync();
        }

        return new ExchangeRateSettingsDto
        {
            AutoUpdateEnabled = settings.AutoUpdateEnabled,
            UpdateIntervalHours = settings.UpdateIntervalHours,
            LastAutoUpdateAt = settings.LastAutoUpdateAt,
            LastFailedAttemptAt = settings.LastFailedAttemptAt,
            LastErrorMessage = settings.LastErrorMessage
        };
    }

    public async Task<ServiceResult> UpdateSettingsAsync(UpdateExchangeRateSettingsDto dto, int userId)
    {
        var settings = (await _uow.ExchangeRateSettings.GetAllAsync()).FirstOrDefault();
        if (settings == null) { settings = new ExchangeRateSettings(); await _uow.ExchangeRateSettings.AddAsync(settings); }

        settings.AutoUpdateEnabled = dto.AutoUpdateEnabled;
        settings.UpdateIntervalHours = dto.UpdateIntervalHours;
        settings.UpdatedBy = userId;
        settings.UpdatedAt = DateTime.UtcNow;
        _uow.ExchangeRateSettings.Update(settings);
        await _uow.SaveChangesAsync();

        return ServiceResult.Ok("Settings updated successfully.");
    }

    private async Task UpdateLastAutoUpdateAsync(bool success, string? error)
    {
        var settings = (await _uow.ExchangeRateSettings.GetAllAsync()).FirstOrDefault();
        if (settings == null) { settings = new ExchangeRateSettings(); await _uow.ExchangeRateSettings.AddAsync(settings); }

        if (success)
        {
            settings.LastAutoUpdateAt = DateTime.UtcNow;
            settings.LastErrorMessage = null;
        }
        else
        {
            settings.LastFailedAttemptAt = DateTime.UtcNow;
            settings.LastErrorMessage = error;
        }
        _uow.ExchangeRateSettings.Update(settings);
        await _uow.SaveChangesAsync();
    }

    public async Task<bool> ShouldAutoUpdateAsync()
    {
        var settings = await GetSettingsAsync();
        if (!settings.AutoUpdateEnabled) return false;

        if (settings.LastAutoUpdateAt == null) return true;

        var elapsed = DateTime.UtcNow - settings.LastAutoUpdateAt.Value;
        return elapsed.TotalHours >= settings.UpdateIntervalHours;
    }
}