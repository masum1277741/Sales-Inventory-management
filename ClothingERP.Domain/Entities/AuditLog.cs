namespace ClothingERP.Domain.Entities;

public class AuditLog : BaseEntity
{

    public string EntityName { get; set; } = string.Empty;

  
    public int? EntityId { get; set; }


    public string ActionType { get; set; } = string.Empty;


    public string UserName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public int UserId { get; set; }

    // Navigation property
    public User? User { get; set; }
}