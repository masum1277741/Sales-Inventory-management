namespace ClothingERP.Application.Interfaces.Services;

public interface IDashboardLayoutService
{
    Task<DashboardLayoutDto> GetLayoutAsync(int userId);
    Task<ServiceResult> SaveLayoutAsync(int userId, SaveDashboardLayoutDto dto);
    Task<ServiceResult> ResetToDefaultAsync(int userId);
    List<WidgetDefinitionDto> GetAvailableWidgets();
}