namespace ClothingERP.Application.Interfaces.Services;

public interface INotificationService
{

    Task CreateAsync(CreateNotificationDto dto);


    Task<NotificationFeedDto> GetFeedAsync(int userId, int take = 20);
    Task<int> GetUnreadCountAsync(int userId);

    Task<ServiceResult> MarkAsReadAsync(int notificationId, int userId);
    Task<ServiceResult> MarkAllAsReadAsync(int userId);
    Task<ServiceResult> DeleteAsync(int notificationId, int userId);


    Task CheckLowStockAlertsAsync();
}