namespace ClothingERP.Application.Interfaces.Services;

public interface ICommissionService
{
    Task<CommissionSettingsDto> GetSettingsAsync();
    Task<ServiceResult> UpdateSettingsAsync(UpdateCommissionSettingsDto dto, int userId);

    Task<IEnumerable<StaffCommissionRateDto>> GetStaffRatesAsync();
    Task<ServiceResult> SetStaffRateAsync(SetStaffRateDto dto, int userId);
    Task<ServiceResult> RemoveStaffRateOverrideAsync(int userId);

    Task CalculateAndRecordCommissionAsync(int staffUserId, int salesInvoiceId, decimal saleAmount, int userId);

    Task ReverseCommissionAsync(int salesInvoiceId, int userId);

    Task<IEnumerable<StaffCommissionSummaryDto>> GetSummaryAsync(DateTime from, DateTime to);
    Task<IEnumerable<CommissionTransactionDto>> GetStaffHistoryAsync(int userId, DateTime from, DateTime to);

    Task<ServiceResult> MarkAsPaidAsync(MarkCommissionPaidDto dto, int paidByUserId);
}