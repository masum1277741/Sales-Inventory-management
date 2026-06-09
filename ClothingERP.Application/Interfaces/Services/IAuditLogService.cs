namespace ClothingERP.Application.Interfaces.Services;

public interface IAuditLogService
{
    Task<IEnumerable<AuditLogDto>> GetAllAsync();

    Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityName, int entityId);

 
    Task LogAsync(int userId,
                  string actionType,
                  string entityName,
                  string? entityId = null,
                  string? ipAddress = null,
                  string? oldValues = null,
                  string? newValues = null,
                  string? description = null);
}