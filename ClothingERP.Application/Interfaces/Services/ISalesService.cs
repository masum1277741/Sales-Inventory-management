namespace ClothingERP.Application.Interfaces.Services;

public interface ISalesService
{
    Task<IEnumerable<SalesInvoiceListDto>> GetAllAsync();
    Task<SalesInvoiceDto?> GetByIdAsync(int id);
    Task<ServiceResult<SalesInvoiceDto>> CreateAsync(CreateSalesInvoiceDto dto, int userId);
    Task<ServiceResult> CancelAsync(int id, string reason, int userId);
    Task<ServiceResult> HoldAsync(int id, int userId);
    Task<ServiceResult> UnholdAsync(int id, int userId);
    Task<IEnumerable<SalesInvoiceListDto>> GetHeldAsync();
    Task<ServiceResult> AddPaymentAsync(int invoiceId, CreateSalesPaymentDto dto, int userId);

    // Dashboard stats
    Task<decimal> GetTodaySalesAsync();
    Task<decimal> GetTodayProfitAsync();
    Task<int> GetTodayInvoiceCountAsync();
}