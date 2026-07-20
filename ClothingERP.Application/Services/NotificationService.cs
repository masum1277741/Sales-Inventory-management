namespace ClothingERP.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;

    public NotificationService(IUnitOfWork uow) => _uow = uow;

    // ── Create ────────────────────────────────────────────────────────────
    public async Task CreateAsync(CreateNotificationDto dto)
    {
     
        if (!string.IsNullOrEmpty(dto.DedupeKey))
        {
            var exists = await _uow.Notifications.GetQueryable()
                .AnyAsync(n => n.DedupeKey == dto.DedupeKey && !n.IsRead && !n.IsDeleted &&
                               n.CreatedAt > DateTime.UtcNow.AddHours(-24));
            if (exists) return;
        }

        await _uow.Notifications.AddAsync(new Notification
        {
            UserId = dto.UserId,
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            Severity = dto.Severity,
            Icon = dto.Icon,
            ActionUrl = dto.ActionUrl,
            DedupeKey = dto.DedupeKey
        });
        await _uow.SaveChangesAsync();
    }
    public async Task CheckCriticalReorderAlertsAsync(IReorderService reorderSvc)
    {
        var suggestions = await reorderSvc.GetSuggestionsAsync();
        var critical = suggestions.Where(s => s.Urgency == "Critical").ToList();

        foreach (var item in critical)
        {
            await CreateAsync(new CreateNotificationDto
            {
                UserId = null,
                Title = "Urgent Reorder Needed",
                Message = $"{item.ProductName} ({item.SizeName}/{item.ColorName}) — মাত্র {item.DaysUntilStockout} দিনের stock বাকি!",
                Type = "LowStock",
                Severity = "danger",
                Icon = "bi-exclamation-octagon",
                ActionUrl = "/Reorder",
                DedupeKey = $"reorder-critical-{item.ProductVariantId}"
            });
        }
    }
    public async Task<NotificationFeedDto> GetFeedAsync(int userId, int take = 20)
    {
        var notifications = await _uow.Notifications.GetQueryable()
            .Where(n => !n.IsDeleted && (n.UserId == userId || n.UserId == null))
            .OrderByDescending(n => n.IsRead == false)
            .ThenByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();

        var unreadCount = await _uow.Notifications.GetQueryable()
            .CountAsync(n => !n.IsDeleted && !n.IsRead && (n.UserId == userId || n.UserId == null));

        return new NotificationFeedDto
        {
            UnreadCount = unreadCount,
            Notifications = notifications.Select(MapToDto).ToList()
        };
    }

    private NotificationDto MapToDto(Notification n) => new()
    {
        Id = n.Id,
        Title = n.Title,
        Message = n.Message,
        Type = n.Type,
        Severity = n.Severity,
        Icon = n.Icon,
        ActionUrl = n.ActionUrl,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt,
        TimeAgo = ToTimeAgo(n.CreatedAt)
    };

    private static string ToTimeAgo(DateTime dt)
    {
        var span = DateTime.UtcNow - dt;
        if (span.TotalMinutes < 1) return "এখনই";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} মিনিট আগে";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} ঘণ্টা আগে";
        if (span.TotalDays < 7) return $"{(int)span.TotalDays} দিন আগে";
        return dt.ToString("dd MMM yyyy");
    }


    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _uow.Notifications.GetQueryable()
            .CountAsync(n => !n.IsDeleted && !n.IsRead && (n.UserId == userId || n.UserId == null));
    }

    // ── Mark as Read ──────────────────────────────────────────────────────
    public async Task<ServiceResult> MarkAsReadAsync(int notificationId, int userId)
    {
        var n = await _uow.Notifications.GetByIdAsync(notificationId);
        if (n == null) return ServiceResult.Fail("Notification not found.");
        if (n.UserId.HasValue && n.UserId != userId)
            return ServiceResult.Fail("Unauthorized.");

        n.IsRead = true;
        n.ReadAt = DateTime.UtcNow;
        _uow.Notifications.Update(n);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Marked as read.");
    }

    public async Task<ServiceResult> MarkAllAsReadAsync(int userId)
    {
        var unread = await _uow.Notifications.GetQueryable()
            .Where(n => !n.IsDeleted && !n.IsRead && (n.UserId == userId || n.UserId == null))
            .ToListAsync();

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            _uow.Notifications.Update(n);
        }
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok($"{unread.Count} notification(s) marked as read.");
    }

    public async Task<ServiceResult> DeleteAsync(int notificationId, int userId)
    {
        var n = await _uow.Notifications.GetByIdAsync(notificationId);
        if (n == null) return ServiceResult.Fail("Notification not found.");
        _uow.Notifications.Remove(n);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Notification removed.");
    }

    public async Task CheckLowStockAlertsAsync()
    {
        var lowStockItems = await _uow.Stocks.GetQueryable()
            .Include(s => s.ProductVariant).ThenInclude(v => v.Product)
            .Where(s => !s.IsDeleted && s.Quantity > 0 &&
                        s.Quantity <= s.ProductVariant.Product.ReorderPoint)
            .ToListAsync();

        foreach (var stock in lowStockItems)
        {
            await CreateAsync(new CreateNotificationDto
            {
                UserId = null,
                Title = "Low Stock Alert",
                Message = $"{stock.ProductVariant.Product.Name} — মাত্র {stock.Quantity} pcs বাকি আছে।",
                Type = "LowStock",
                Severity = "warning",
                Icon = "bi-exclamation-triangle",
                ActionUrl = "/Stock",
                DedupeKey = $"lowstock-variant-{stock.ProductVariantId}"
            });
        }


        var outOfStock = await _uow.Stocks.GetQueryable()
            .Include(s => s.ProductVariant).ThenInclude(v => v.Product)
            .Where(s => !s.IsDeleted && s.Quantity == 0)
            .ToListAsync();

        foreach (var stock in outOfStock)
        {
            await CreateAsync(new CreateNotificationDto
            {
                UserId = null,
                Title = "Out of Stock",
                Message = $"{stock.ProductVariant.Product.Name} stock শেষ হয়ে গেছে!",
                Type = "LowStock",
                Severity = "danger",
                Icon = "bi-x-circle",
                ActionUrl = "/Stock",
                DedupeKey = $"outofstock-variant-{stock.ProductVariantId}"
            });
        }
    }
}