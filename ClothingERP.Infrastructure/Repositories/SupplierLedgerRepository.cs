namespace ClothingERP.Infrastructure.Repositories;

public class SupplierLedgerRepository : GenericRepository<SupplierLedger>, ISupplierLedgerRepository
{
    public SupplierLedgerRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<SupplierLedger>> GetBySupplierIdAsync(int supplierId, DateTime? from = null, DateTime? to = null)
    {
        var q = _dbSet.Where(l => l.SupplierId == supplierId);
        if (from.HasValue) q = q.Where(l => l.EntryDate >= from.Value);
        if (to.HasValue) q = q.Where(l => l.EntryDate <= to.Value.AddDays(1));
        return await q.OrderByDescending(l => l.EntryDate).ToListAsync();
    }

    public async Task<decimal> GetCurrentBalanceAsync(int supplierId)
    {
        var last = await _dbSet.Where(l => l.SupplierId == supplierId)
                               .OrderByDescending(l => l.Id).FirstOrDefaultAsync();
        return last?.Balance ?? 0;
    }

    public async Task<SupplierLedger?> GetLastEntryAsync(int supplierId)
        => await _dbSet.Where(l => l.SupplierId == supplierId)
                       .OrderByDescending(l => l.Id).FirstOrDefaultAsync();
}