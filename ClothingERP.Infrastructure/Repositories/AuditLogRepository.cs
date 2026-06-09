namespace ClothingERP.Infrastructure.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(
        int userId, DateTime? from = null, DateTime? to = null)
    {
        var q = _dbSet.Include(l => l.User).Where(l => l.UserId == userId);

   
        if (from.HasValue) q = q.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue) q = q.Where(l => l.CreatedAt <= to.Value.AddDays(1));

        return await q.OrderByDescending(l => l.CreatedAt).Take(100).ToListAsync();
    }

   
    public async Task<IEnumerable<AuditLog>> GetByTableNameAsync(string tableName)
        => await _dbSet.Include(l => l.User)
                       .Where(l => l.EntityName == tableName)
                       .OrderByDescending(l => l.CreatedAt)
                       .Take(100)
                       .ToListAsync();

    public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(
        DateTime from, DateTime to, int? userId = null)
    {
        var q = _dbSet.Include(l => l.User)
                      .Where(l => l.CreatedAt >= from && l.CreatedAt <= to.AddDays(1));

        if (userId.HasValue) q = q.Where(l => l.UserId == userId.Value);

        return await q.OrderByDescending(l => l.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetFailedLoginsAsync()

        => await _dbSet.Include(l => l.User)
                       .Where(l => l.ActionType == "LoginFailed")
                       .OrderByDescending(l => l.CreatedAt)
                       .Take(50)
                       .ToListAsync();
}