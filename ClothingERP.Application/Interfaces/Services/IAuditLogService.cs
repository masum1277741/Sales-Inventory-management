namespace ClothingERP.Application.Interfaces.Services;

public interface IAuditLogService
{
    Task LogAsync(int userId, string action, string tableName,
                  string? recordId = null, string? oldValues = null, string? newValues = null,
                  string? ipAddress = null, bool isSuccess = true, string? errorMessage = null);

    Task<IEnumerable<AuditLogDto>> GetLogsAsync(DateTime from, DateTime to, int? userId = null);
    Task<IEnumerable<AuditLogDto>> GetFailedLoginsAsync(int count = 50);
    Task<IEnumerable<AuditLogDto>> GetByUserAsync(int userId, int count = 50);
}