namespace ClothingERP.Application.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
}

public class CreateNotificationDto
{
    public int? UserId { get; set; }       // null = broadcast (Admin/Manager দের কাছে যাবে)
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info";
    public string Severity { get; set; } = "info";
    public string Icon { get; set; } = "bi-bell";
    public string? ActionUrl { get; set; }
    public string? DedupeKey { get; set; }
}

public class NotificationFeedDto
{
    public int UnreadCount { get; set; }
    public List<NotificationDto> Notifications { get; set; } = new();
}