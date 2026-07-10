namespace ClothingERP.Web.Controllers;

public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboard;
    private readonly ICommissionService _commissionSvc;
    private readonly IDashboardLayoutService _layoutSvc;

    public DashboardController(IDashboardService dashboard, ICommissionService commissionSvc, IDashboardLayoutService layoutSvc)
        => (_dashboard, _commissionSvc, _layoutSvc) = (dashboard, commissionSvc, layoutSvc);

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard";
        var data = await _dashboard.GetDashboardDataAsync();

        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var commissionSummary = await _commissionSvc.GetSummaryAsync(monthStart, DateTime.Today);
        ViewBag.MyPendingCommission = commissionSummary
            .FirstOrDefault(c => c.UserId == CurrentUserId)?.PendingCommission ?? 0;


        var layout = await _layoutSvc.GetLayoutAsync(CurrentUserId);
        ViewBag.WidgetLayout = layout.Widgets.OrderBy(w => w.Order).ToList();
        ViewBag.AvailableWidgets = _layoutSvc.GetAvailableWidgets();

        return View(data);
    }

    // ── AJAX: Save Custom Layout ───────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveLayout([FromBody] SaveDashboardLayoutDto dto)
    {
        var r = await _layoutSvc.SaveLayoutAsync(CurrentUserId, dto);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetLayout()
    {
        var r = await _layoutSvc.ResetToDefaultAsync(CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }
}