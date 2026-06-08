namespace ClothingERP.Infrastructure.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId, DateTime? from = null, DateTime? to = null)
    {
        var q = _dbSet.Include(l => l.User).Where(l => l.UserId == userId);
        if (from.HasValue) q = q.Where(l => l.ActionDate >= from.Value);
        if (to.HasValue) q = q.Where(l => l.ActionDate <= to.Value.AddDays(1));
        return await q.OrderByDescending(l => l.ActionDate).Take(100).ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByTableNameAsync(string tableName)
        => await _dbSet.Include(l => l.User)
                       .Where(l => l.TableName == tableName)
                       .OrderByDescending(l => l.ActionDate).Take(100).ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to, int? userId = null)
    {
        var q = _dbSet.Include(l => l.User)
                      .Where(l => l.ActionDate >= from && l.ActionDate <= to.AddDays(1));
        if (userId.HasValue) q = q.Where(l => l.UserId == userId.Value);
        return await q.OrderByDescending(l => l.ActionDate).ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetFailedLoginsAsync()
        => await _dbSet.Include(l => l.User)
                       .Where(l => l.Action == "Login" && !l.IsSuccess)
                       .OrderByDescending(l => l.ActionDate).Take(50).ToListAsync();
}