namespace ClothingERP.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AuditLogService(IUnitOfWork uow, IMapper mapper)
        => (_uow, _mapper) = (uow, mapper);

    // ── Get All ───────────────────────────────────────────────────────────
    public async Task<IEnumerable<AuditLogDto>> GetAllAsync()
    {
        var logs = await _uow.AuditLogs.GetAllAsync();
        return _mapper.Map<IEnumerable<AuditLogDto>>(
            logs.OrderByDescending(l => l.CreatedAt));
    }

    // ── Get By Entity ─────────────────────────────────────────────────────
    public async Task<IEnumerable<AuditLogDto>> GetByEntityAsync(
        string entityName, int entityId)
    {
        var logs = await _uow.AuditLogs.GetAllAsync();
        return _mapper.Map<IEnumerable<AuditLogDto>>(
            logs.Where(l => l.EntityName == entityName && l.EntityId == entityId)
                .OrderByDescending(l => l.CreatedAt));
    }

    // ── Log Action ────────────────────────────────────────────────────────
    public async Task LogAsync(int userId,
                                string actionType,
                                string entityName,
                                string? entityId = null,
                                string? ipAddress = null,
                                string? oldValues = null,
                                string? newValues = null,
                                string? description = null)
    {
       
        var user = await _uow.Users.GetByIdAsync(userId);
        var userName = user?.Username ?? $"User#{userId}";

     
        int? parsedEntityId = int.TryParse(entityId, out var eid) ? eid : null;

        var log = new AuditLog
        {
            EntityName = entityName,
            EntityId = parsedEntityId,
            ActionType = actionType,
            UserId = userId,
            UserName = userName,
            IPAddress = ipAddress,
            OldValues = oldValues,
            NewValues = newValues,
            Description = description
        };

        await _uow.AuditLogs.AddAsync(log);
        await _uow.SaveChangesAsync();
    }
}