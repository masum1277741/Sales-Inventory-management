namespace ClothingERP.Web.Controllers;

[Authorize(Roles = "Administrator")]   
public class BranchController : BaseController
{
    private readonly IBranchService _branchSvc;
    private readonly ICurrentBranchProvider _currentBranchProvider;

    public BranchController(IBranchService branchSvc, ICurrentBranchProvider currentBranchProvider)
        => (_branchSvc, _currentBranchProvider) = (branchSvc, currentBranchProvider);

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Branches";
        var branches = await _branchSvc.GetAllAsync();
        return View(branches);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Add Branch";
        return View(new CreateBranchDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBranchDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await _branchSvc.CreateAsync(dto, CurrentUserId);
        if (!result.Success) { ModelState.AddModelError("", result.Message!); return View(dto); }
        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var r = await _branchSvc.ToggleStatusAsync(id, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }
    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> MyAccess()
    {
        var roleName = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
        var access = await _branchSvc.GetUserAccessAsync(CurrentUserId, roleName);
        return Json(access);
    }
    // ── Stock Comparison Across Branches ─────────────────────────────────
    public async Task<IActionResult> StockComparison(string? keyword)
    {
        ViewData["Title"] = "Branch Stock Comparison";
        var comparison = await _branchSvc.CompareStockAcrossBranchesAsync(keyword);
        return View(comparison);
    }

    // ── User Branch Assignment ────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignUser(UserBranchAssignmentDto dto)
    {
        var r = await _branchSvc.AssignUserToBranchesAsync(dto, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

 
}