using System.Data;

namespace ClothingERP.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImage { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public int RoleId { get; set; }

    public virtual Role Role { get; set; } = null!;
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}