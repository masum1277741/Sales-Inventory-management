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

    public async Task<IActionResult> Index(int? branchId = null, bool allBranches = false)
    {
        ViewData["Title"] = "Dashboard";

        // ── Branch filter logic ────────────────────────────────────────────
        var myBranchId = _branchProvider.GetCurrentBranchId();
        var roleName = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
        var isAdmin = roleName.Equals("Administrator", StringComparison.OrdinalIgnoreCase);


        int? filterBranchId;
        if (isAdmin && allBranches)
            filterBranchId = null;  // null = all branches
        else if (isAdmin && branchId.HasValue)
            filterBranchId = branchId;
        else
            filterBranchId = myBranchId;

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


        ViewBag.AllBranches = isAdmin;
        ViewBag.Branches = isAdmin ? await _branchSvc.GetAllAsync() : null;
        ViewBag.SelectedBranch = filterBranchId;
        ViewBag.ShowAllBranches = allBranches && isAdmin;

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