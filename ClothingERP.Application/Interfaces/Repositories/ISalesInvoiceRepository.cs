namespace ClothingERP.Application.Interfaces.Repositories;

public interface ISalesInvoiceRepository : IRepository<SalesInvoice>
{
    Task<SalesInvoice?> GetWithDetailsAsync(int id);
    Task<IEnumerable<SalesInvoice>> GetByCustomerAsync(int customerId);
    Task<IEnumerable<SalesInvoice>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<SalesInvoice>> GetHeldInvoicesAsync();
    Task<string> GenerateInvoiceNumberAsync();
    Task<decimal> GetTodaySalesAmountAsync();
    Task<decimal> GetTodayProfitAsync();
    Task<int> GetTodayInvoiceCountAsync();
    Task<List<MonthlySalesData>> GetMonthlySalesAsync(int year);
}