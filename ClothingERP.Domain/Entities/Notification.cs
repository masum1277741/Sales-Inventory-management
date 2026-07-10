namespace ClothingERP.Domain.Entities;

public class Notification : BaseEntity
{
    public int? UserId { get; set; }   // null হলে সবার জন্য (broadcast — Admin/Manager)
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info";   // LowStock, BigSale, PendingApproval, Payment, System
    public string Severity { get; set; } = "info";    // info, success, warning, danger
    public string Icon { get; set; } = "bi-bell";
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public string? DedupeKey { get; set; }   // একই alert ডুপ্লিকেট না হওয়ার জন্য (যেমন: "lowstock-variant-42")

    public User? User { get; set; }
}