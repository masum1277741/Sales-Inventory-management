namespace ClothingERP.Application.Interfaces.Services;

public interface IDemandForecastService
{
    Task<ForecastSettingsDto> GetSettingsAsync();
    Task<ServiceResult> UpdateSettingsAsync(UpdateForecastSettingsDto dto, int userId);

   
    Task<DemandForecastDto> ForecastForVariantAsync(int variantId);


    Task<ForecastSummaryDto> GetSummaryAsync(int topCount = 10);


    Task<List<TopMoverDto>> SearchForecastsAsync(string keyword);
}