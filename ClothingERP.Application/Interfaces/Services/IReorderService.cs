namespace ClothingERP.Application.Interfaces.Services;

public interface IReorderService
{
    Task<List<ReorderSuggestionDto>> GetSuggestionsAsync();
    Task<ReorderSummaryDto> GetSummaryAsync();

    Task<ReorderSettingsDto> GetSettingsAsync();
    Task<ServiceResult> UpdateSettingsAsync(UpdateReorderSettingsDto dto, int userId);

    Task<ServiceResult<int>> GeneratePurchaseOrderAsync(GeneratePOFromSuggestionsDto dto, int userId);
}