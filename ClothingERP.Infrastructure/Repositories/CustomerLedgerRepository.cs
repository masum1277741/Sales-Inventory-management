namespace ClothingERP.Infrastructure.Repositories;

public class CustomerLedgerRepository : GenericRepository<CustomerLedger>, ICustomerLedgerRepository
{
    public CustomerLedgerRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<CustomerLedger>> GetByCustomerIdAsync(int customerId, DateTime? from = null, DateTime? to = null)
    {
        var q = _dbSet.Where(l => l.CustomerId == customerId);
        if (from.HasValue) q = q.Where(l => l.EntryDate >= from.Value);
        if (to.HasValue) q = q.Where(l => l.EntryDate <= to.Value.AddDays(1));
        return await q.OrderByDescending(l => l.EntryDate).ToListAsync();
    }

    public async Task<decimal> GetCurrentBalanceAsync(int customerId)
    {
        var last = await _dbSet.Where(l => l.CustomerId == customerId)
                               .OrderByDescending(l => l.EntryDate)
                               .ThenByDescending(l => l.Id)
                               .FirstOrDefaultAsync();
        return last?.Balance ?? 0;
    }

    public async Task<CustomerLedger?> GetLastEntryAsync(int customerId)
        => await _dbSet.Where(l => l.CustomerId == customerId)
                       .OrderByDescending(l => l.Id)
                       .FirstOrDefaultAsync();
}