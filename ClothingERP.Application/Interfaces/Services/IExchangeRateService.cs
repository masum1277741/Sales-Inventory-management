namespace ClothingERP.Application.Interfaces.Services;

public interface IExchangeRateService
{
  
    Task<CurrentRatesDto> GetCurrentRatesAsync();


    Task<RefreshResultDto> RefreshFromApiAsync();


    Task<ServiceResult> SetManualRateAsync(ManualRateOverrideDto dto, int userId);

    Task<IEnumerable<ExchangeRateHistoryDto>> GetHistoryAsync(int take = 30);

    Task<ExchangeRateSettingsDto> GetSettingsAsync();
    Task<ServiceResult> UpdateSettingsAsync(UpdateExchangeRateSettingsDto dto, int userId);


    Task<bool> ShouldAutoUpdateAsync();
}