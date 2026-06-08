namespace ClothingERP.Infrastructure.Repositories;

public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
{
    public SupplierRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Supplier?> GetWithDetailsAsync(int supplierId)
        => await _dbSet.FirstOrDefaultAsync(s => s.Id == supplierId);

    public async Task<IEnumerable<Supplier>> GetWithDueBalanceAsync()
        => await _dbSet.Where(s => s.CurrentBalance > 0 && s.IsActive)
                       .OrderByDescending(s => s.CurrentBalance).ToListAsync();

    public async Task<IEnumerable<Supplier>> SearchAsync(string keyword)
        => await _dbSet.Where(s => s.IsActive &&
                              (s.CompanyName.Contains(keyword) || s.PhoneNumber.Contains(keyword)))
                       .Take(20).ToListAsync();

    public async Task<bool> IsPhoneExistsAsync(string phone, int? excludeId = null)
    {
        var q = _dbSet.Where(s => s.PhoneNumber == phone);
        if (excludeId.HasValue) q = q.Where(s => s.Id != excludeId.Value);
        return await q.AnyAsync();
    }
}