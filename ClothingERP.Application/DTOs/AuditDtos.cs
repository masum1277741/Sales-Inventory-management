namespace ClothingERP.Application.DTOs;

public class AuditLogDto
{
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? IPAddress { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; }
}

public class CreateAuditLogDto
{
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? IPAddress { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Description { get; set; }
}