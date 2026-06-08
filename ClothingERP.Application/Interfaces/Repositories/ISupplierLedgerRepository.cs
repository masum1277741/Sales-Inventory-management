namespace ClothingERP.Application.Interfaces.Repositories;

public interface ISupplierLedgerRepository : IRepository<SupplierLedger>
{
    Task<IEnumerable<SupplierLedger>> GetBySupplierIdAsync(int supplierId, DateTime? from = null, DateTime? to = null);
    Task<decimal> GetCurrentBalanceAsync(int supplierId);
    Task<SupplierLedger?> GetLastEntryAsync(int supplierId);
}