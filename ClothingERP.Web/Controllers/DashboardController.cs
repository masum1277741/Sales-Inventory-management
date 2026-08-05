namespace ClothingERP.Web.Controllers;

public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboard;
    private readonly ICommissionService _commissionSvc;
    private readonly IDashboardLayoutService _layoutSvc;
    private readonly ICurrentBranchProvider _branchProvider;
    private readonly IBranchService _branchSvc;
    private readonly IExchangeRateService _rateSvc;

    public DashboardController(
        IDashboardService dashboard,
        ICommissionService commissionSvc,
        IDashboardLayoutService layoutSvc,
        ICurrentBranchProvider branchProvider,
        IBranchService branchSvc,
        IExchangeRateService rateSvc)
        => (_dashboard, _commissionSvc, _layoutSvc, _branchProvider, _branchSvc, _rateSvc)
            = (dashboard, commissionSvc, layoutSvc, branchProvider, branchSvc, rateSvc);

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard";

        var filterBranchId = _branchProvider.GetCurrentBranchId();
        var data = await _dashboard.GetDashboardDataAsync(filterBranchId);

        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var commissionSummary = await _commissionSvc.GetSummaryAsync(monthStart, DateTime.Today);
        ViewBag.MyPendingCommission = commissionSummary
            .FirstOrDefault(c => c.UserId == CurrentUserId)?.PendingCommission ?? 0;


        var rates = await _rateSvc.GetCurrentRatesAsync();
        ViewBag.RateBDT = rates.UsdToBdt;

        var layout = await _layoutSvc.GetLayoutAsync(CurrentUserId);
        ViewBag.WidgetLayout = layout.Widgets.OrderBy(w => w.Order).ToList();
        ViewBag.AvailableWidgets = _layoutSvc.GetAvailableWidgets();


        ViewBag.AllBranches = false;
        ViewBag.Branches = null;
        ViewBag.SelectedBranch = filterBranchId;
        ViewBag.ShowAllBranches = false;

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