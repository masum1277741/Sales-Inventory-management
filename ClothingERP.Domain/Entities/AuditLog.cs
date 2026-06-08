namespace ClothingERP.Domain.Entities;

public class AuditLog : BaseEntity
{
    public int UserId { get; set; }
    public string Action { get; set; } = string.Empty;      // Create, Update, Delete, Login
    public string TableName { get; set; } = string.Empty;
    public string? RecordId { get; set; }
    public string? OldValues { get; set; }                  // JSON
    public string? NewValues { get; set; }                  // JSON
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime ActionDate { get; set; } = DateTime.UtcNow;
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }

    public virtual User User { get; set; } = null!;
}