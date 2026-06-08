namespace ClothingERP.Application.Interfaces.Repositories;

public interface ICustomerLedgerRepository : IRepository<CustomerLedger>
{
    Task<IEnumerable<CustomerLedger>> GetByCustomerIdAsync(int customerId, DateTime? from = null, DateTime? to = null);
    Task<decimal> GetCurrentBalanceAsync(int customerId);
    Task<CustomerLedger?> GetLastEntryAsync(int customerId);
}