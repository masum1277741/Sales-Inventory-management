namespace ClothingERP.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AuditLogService(IUnitOfWork uow, IMapper mapper)
        => (_uow, _mapper) = (uow, mapper);

    public async Task LogAsync(int userId, string action, string tableName,
        string? recordId = null, string? oldValues = null, string? newValues = null,
        string? ipAddress = null, bool isSuccess = true, string? errorMessage = null)
    {
        if (userId <= 0) return;
        try
        {
            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                UserId = userId,
                Action = action,
                TableName = tableName,
                RecordId = recordId,
                OldValues = oldValues,
                NewValues = newValues,
                IPAddress = ipAddress,
                ActionDate = DateTime.UtcNow,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage,
                CreatedBy = userId
            });
            await _uow.SaveChangesAsync();
        }
        catch { /* audit failure must never break main flow */ }
    }

    public async Task<IEnumerable<AuditLogDto>> GetLogsAsync(DateTime from, DateTime to, int? userId = null)
        => _mapper.Map<IEnumerable<AuditLogDto>>(await _uow.AuditLogs.GetByDateRangeAsync(from, to, userId));

    public async Task<IEnumerable<AuditLogDto>> GetFailedLoginsAsync(int count = 50)
        => _mapper.Map<IEnumerable<AuditLogDto>>(await _uow.AuditLogs.GetFailedLoginsAsync());

    public async Task<IEnumerable<AuditLogDto>> GetByUserAsync(int userId, int count = 50)
        => _mapper.Map<IEnumerable<AuditLogDto>>((await _uow.AuditLogs.GetByUserIdAsync(userId)).Take(count));
}