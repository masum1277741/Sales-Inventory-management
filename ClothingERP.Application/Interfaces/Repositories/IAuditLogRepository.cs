namespace ClothingERP.Application.Interfaces.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId, DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<AuditLog>> GetByTableNameAsync(string tableName);
    Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime from, DateTime to, int? userId = null);
    Task<IEnumerable<AuditLog>> GetFailedLoginsAsync();
}