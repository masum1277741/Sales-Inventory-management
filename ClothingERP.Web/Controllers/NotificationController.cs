namespace ClothingERP.Web.Controllers;

public class NotificationController : BaseController
{
    private readonly INotificationService _notificationSvc;
    private readonly IReorderService _reorderSvc;
    public NotificationController(INotificationService notificationSvc, IReorderService reorderSvc) => (_notificationSvc, _reorderSvc) = (notificationSvc, reorderSvc);

    // ── Full Page List ────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Notifications";
        var feed = await _notificationSvc.GetFeedAsync(CurrentUserId, take: 100);
        return View(feed.Notifications);
    }

    // ── AJAX: Feed for bell dropdown ──────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetFeed()
    {

        await _notificationSvc.CheckLowStockAlertsAsync();
        await _notificationSvc.CheckCriticalReorderAlertsAsync(_reorderSvc);
        var feed = await _notificationSvc.GetFeedAsync(CurrentUserId, take: 10);
        return Json(feed);
    }


    [HttpGet]
    public async Task<IActionResult> UnreadCount()
    {
        var count = await _notificationSvc.GetUnreadCountAsync(CurrentUserId);
        return Json(new { count });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var r = await _notificationSvc.MarkAsReadAsync(id, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var r = await _notificationSvc.MarkAllAsReadAsync(CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _notificationSvc.DeleteAsync(id, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }
}