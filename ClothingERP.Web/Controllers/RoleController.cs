namespace ClothingERP.Web.Controllers;

public class RoleController : BaseController
{
    private readonly IRoleService _roles;

    public RoleController(IRoleService roles) => _roles = roles;

    // ── Index ─────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Role Management";
        return View(await _roles.GetAllAsync());
    }

    // ── Create ────────────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Add Role";
        return View(new CreateRoleDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoleDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await _roles.CreateAsync(dto, CurrentUserId);
        if (!result.Success) { ModelState.AddModelError("", result.Message!); return View(dto); }
        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Index));
    }

    // ── Edit ──────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Role";
        var role = await _roles.GetByIdAsync(id);
        if (role == null) return NotFound();
        ViewBag.Role = role;
        return View(new CreateRoleDto { Name = role.Name, Description = role.Description, IsActive = role.IsActive });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateRoleDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await _roles.UpdateAsync(id, dto, CurrentUserId);
        if (!result.Success) { ModelState.AddModelError("", result.Message!); return View(dto); }
        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Index));
    }

    // ── Delete ────────────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _roles.DeleteAsync(id);
        return result.Success ? JsonSuccess(message: result.Message!) : JsonError(result.Message!);
    }

    // ── Permissions ───────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Permissions(int id)
    {
        ViewData["Title"] = "Role Permissions";
        var role = await _roles.GetByIdAsync(id);
        if (role == null) return NotFound();
        ViewBag.Role = role;
        return View((await _roles.GetAllModulesWithPermissionsAsync(id)).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> SavePermissions(int id, [FromBody] List<SavePermissionDto> permissions)
    {
        if (permissions == null || !permissions.Any())
            return JsonError("No permissions data received.");
        var result = await _roles.SavePermissionsAsync(id, permissions, CurrentUserId);
        return result.Success ? JsonSuccess(message: result.Message!) : JsonError(result.Message ?? "Save failed.");
    }
}