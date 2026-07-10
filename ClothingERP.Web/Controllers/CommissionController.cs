namespace ClothingERP.Web.Controllers;

public class CommissionController : BaseController
{
    private readonly ICommissionService _commissionSvc;

    public CommissionController(ICommissionService commissionSvc) => _commissionSvc = commissionSvc;

    // ── Index — সব staff এর summary ─────────────────────────────────────
    public async Task<IActionResult> Index(DateTime? fromDate = null, DateTime? toDate = null)
    {
        ViewData["Title"] = "Staff Commission";
        var from = fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var to = toDate ?? DateTime.Today;

        var summary = (await _commissionSvc.GetSummaryAsync(from, to)).ToList();

        ViewBag.FromDate = from.ToString("yyyy-MM-dd");
        ViewBag.ToDate = to.ToString("yyyy-MM-dd");
        ViewBag.TotalCommission = summary.Sum(s => s.TotalCommission);
        ViewBag.TotalPending = summary.Sum(s => s.PendingCommission);
        ViewBag.TotalPaid = summary.Sum(s => s.PaidCommission);
        ViewBag.TotalSalesAmount = summary.Sum(s => s.TotalSalesAmount);

        return View(summary);
    }

    // ── Staff Details (drill-down) ───────────────────────────────────────
    public async Task<IActionResult> StaffDetails(int userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        ViewData["Title"] = "Commission Details";
        var from = fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var to = toDate ?? DateTime.Today;

        var history = (await _commissionSvc.GetStaffHistoryAsync(userId, from, to)).ToList();

        ViewBag.FromDate = from.ToString("yyyy-MM-dd");
        ViewBag.ToDate = to.ToString("yyyy-MM-dd");
        ViewBag.UserId = userId;
        ViewBag.UserName = history.FirstOrDefault()?.UserName ?? "Staff";
        ViewBag.Pending = history.Where(h => h.Status == "Pending").Sum(h => h.CommissionAmount);
        ViewBag.Paid = history.Where(h => h.Status == "Paid").Sum(h => h.CommissionAmount);

        return View(history);
    }

    // ── Settings ──────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        ViewData["Title"] = "Commission Settings";
        ViewBag.Settings = await _commissionSvc.GetSettingsAsync();
        var rates = await _commissionSvc.GetStaffRatesAsync();
        return View(rates);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSettings(UpdateCommissionSettingsDto dto)
    {
        var r = await _commissionSvc.UpdateSettingsAsync(dto, CurrentUserId);
        SetSuccess(r.Message!);
        return RedirectToAction(nameof(Settings));
    }

    // ── AJAX: Set / Remove Custom Rate ───────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStaffRate(SetStaffRateDto dto)
    {
        var r = await _commissionSvc.SetStaffRateAsync(dto, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveStaffRate(int userId)
    {
        var r = await _commissionSvc.RemoveStaffRateOverrideAsync(userId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    // ── AJAX: Mark As Paid ────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsPaid([FromBody] MarkCommissionPaidDto dto)
    {
        var r = await _commissionSvc.MarkAsPaidAsync(dto, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }
}