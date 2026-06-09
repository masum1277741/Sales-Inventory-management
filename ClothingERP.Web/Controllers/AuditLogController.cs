namespace ClothingERP.Web.Controllers;

public class AuditLogController : BaseController
{
    private readonly IAuditLogService _auditSvc;
    private readonly IUserService _userSvc;

    public AuditLogController(IAuditLogService auditSvc, IUserService userSvc)
        => (_auditSvc, _userSvc) = (auditSvc, userSvc);

    public async Task<IActionResult> Index(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? action = null,
        string? entity = null,
        int? userId = null)
    {
        ViewData["Title"] = "Audit Log";

        var all = (await _auditSvc.GetAllAsync()).ToList();
        var from = fromDate ?? DateTime.Today.AddDays(-7);
        var to = toDate ?? DateTime.Today;

        // ── Filter ────────────────────────────────────────────────────────
        IEnumerable<AuditLogDto> filtered = all
            .Where(a => a.Timestamp.Date >= from &&
                        a.Timestamp.Date <= to);

        if (!string.IsNullOrEmpty(action)) filtered = filtered.Where(a => a.ActionType == action);
        if (!string.IsNullOrEmpty(entity)) filtered = filtered.Where(a => a.EntityName == entity);
        if (userId.HasValue) filtered = filtered.Where(a => a.UserId == userId);

        var result = filtered.OrderByDescending(a => a.Timestamp).ToList();

        // ── ViewBag ───────────────────────────────────────────────────────
        ViewBag.FromDate = from.ToString("yyyy-MM-dd");
        ViewBag.ToDate = to.ToString("yyyy-MM-dd");
        ViewBag.Action = action;
        ViewBag.Entity = entity;
        ViewBag.UserId = userId;
        ViewBag.TotalCount = all.Count;
        ViewBag.TodayCount = all.Count(a => a.Timestamp.Date == DateTime.Today);
        ViewBag.ErrorCount = all.Count(a => a.ActionType is "Error" or "LoginFailed");
        ViewBag.Actions = all.Select(a => a.ActionType)
                                 .Distinct()
                                 .OrderBy(x => x)
                                 .ToList();
        ViewBag.Entities = all.Select(a => a.EntityName)
                                 .Distinct()
                                 .OrderBy(x => x)
                                 .ToList();
        ViewBag.Users = await _userSvc.GetAllAsync();

        return View(result);
    }
}