using ClothingERP.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class BranchController : BaseController
{
    private readonly IBranchService _branchSvc;

    public BranchController(IBranchService branchSvc) => _branchSvc = branchSvc;

    // ── Index ─────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Branches";
        var branches = await _branchSvc.GetAllAsync();
        return View(branches);
    }

    // ── Create ────────────────────────────────────────────────────────────
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

    // ── Edit (GET) ────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Branch";
        var branch = await _branchSvc.GetByIdAsync(id);
        if (branch == null) return NotFound();

        var dto = new CreateBranchDto
        {
            Code = branch.Code,
            Name = branch.Name,
            Address = branch.Address,
            PhoneNumber = branch.PhoneNumber,
            Country = branch.Country,
            IsActive = branch.IsActive
        };
        ViewBag.BranchId = id;
        ViewBag.IsMainBranch = branch.IsMainBranch;
        return View(dto);
    }

    // ── Edit (POST) ───────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateBranchDto dto)
    {
        ViewBag.BranchId = id;
        if (!ModelState.IsValid) return View(dto);

        var result = await _branchSvc.UpdateAsync(id, dto, CurrentUserId);
        if (!result.Success) { ModelState.AddModelError("", result.Message!); return View(dto); }
        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Index));
    }

    // ── Toggle Status ─────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var r = await _branchSvc.ToggleStatusAsync(id, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    // ── Stock Comparison ──────────────────────────────────────────────────
    public async Task<IActionResult> StockComparison(string? keyword)
    {
        ViewData["Title"] = "Branch Stock Comparison";
        var comparison = await _branchSvc.CompareStockAcrossBranchesAsync(keyword);
        ViewBag.Keyword = keyword;
        return View(comparison);
    }
}