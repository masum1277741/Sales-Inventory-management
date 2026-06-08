namespace ClothingERP.Infrastructure.Repositories;

public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Customer?> GetWithDetailsAsync(int customerId)
        => await _dbSet.Include(c => c.CustomerGroup)
                       .FirstOrDefaultAsync(c => c.Id == customerId);

    public async Task<IEnumerable<Customer>> GetWithDueBalanceAsync()
        => await _dbSet.Include(c => c.CustomerGroup)
                       .Where(c => c.CurrentBalance > 0 && c.IsActive)
                       .OrderByDescending(c => c.CurrentBalance)
                       .ToListAsync();

    public async Task<IEnumerable<Customer>> SearchAsync(string keyword)
        => await _dbSet.Include(c => c.CustomerGroup)
                       .Where(c => c.IsActive && (c.Name.Contains(keyword) ||
                              (c.PhoneNumber != null && c.PhoneNumber.Contains(keyword))))
                       .Take(20).ToListAsync();

    public async Task<Customer?> GetByPhoneAsync(string phone)
        => await _dbSet.Include(c => c.CustomerGroup)
                       .FirstOrDefaultAsync(c => c.PhoneNumber == phone);

    public async Task<bool> IsPhoneExistsAsync(string phone, int? excludeId = null)
    {
        var q = _dbSet.Where(c => c.PhoneNumber == phone);
        if (excludeId.HasValue) q = q.Where(c => c.Id != excludeId.Value);
        return await q.AnyAsync();
    }
}