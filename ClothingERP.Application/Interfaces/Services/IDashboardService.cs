namespace ClothingERP.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardDataAsync(int? branchId = null);
}