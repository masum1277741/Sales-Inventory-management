namespace ClothingERP.Application.Interfaces.Repositories;

public interface ISalesReturnRepository : IRepository<SalesReturn>
{
    Task<SalesReturn?> GetWithDetailsAsync(int id);
    Task<IEnumerable<SalesReturn>> GetByInvoiceIdAsync(int invoiceId);
    Task<IEnumerable<SalesReturn>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<string> GenerateReturnNumberAsync();
}