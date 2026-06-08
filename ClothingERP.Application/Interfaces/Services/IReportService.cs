namespace ClothingERP.Application.Interfaces.Services;

public interface IReportService
{
    Task<IEnumerable<SalesReportItemDto>> GetSalesReportAsync(DateTime from, DateTime to, int? customerId = null);
    Task<IEnumerable<StockReportItemDto>> GetStockReportAsync(int? categoryId = null, bool? lowStockOnly = null);
    Task<IEnumerable<CustomerLedgerDto>> GetCustomerLedgerAsync(int customerId, DateTime from, DateTime to);
    Task<IEnumerable<SupplierLedgerDto>> GetSupplierLedgerAsync(int supplierId, DateTime from, DateTime to);
    Task<ProfitLossDto> GetProfitLossAsync(DateTime from, DateTime to);
    Task<IEnumerable<CustomerDueDto>> GetCustomerDueListAsync();
    Task<IEnumerable<SupplierDueDto>> GetSupplierDueListAsync();
    Task<IEnumerable<SalesReturnReportItemDto>> GetReturnReportAsync(DateTime from, DateTime to);
    Task<IEnumerable<PurchaseReportItemDto>> GetPurchaseReportAsync(DateTime from, DateTime to, int? supplierId = null);
    Task<decimal> GetSalesReturnsTotalAsync(DateTime from, DateTime to);
    Task<decimal> GetCOGSAsync(DateTime from, DateTime to);
}