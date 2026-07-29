using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClothingERP.Web.Controllers;

[Authorize(Roles = "Administrator")]
public class UserController : BaseController
{
    private readonly IUserService _userSvc;
    private readonly IRoleService _roleSvc;
    private readonly IBranchService _branchSvc;

    public UserController(IUserService userSvc, IRoleService roleSvc, IBranchService branchSvc)
        => (_userSvc, _roleSvc, _branchSvc) = (userSvc, roleSvc, branchSvc);

    // ── INDEX ─────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Users";
        var users = await _userSvc.GetAllAsync();
        return View(users);
    }

    // ── CREATE (GET) ──────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Add User";
        await LoadDropdowns();
        return View(new CreateUserDto());
    }

    // ── CREATE (POST) ─────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            return View(dto);
        }

        var result = await _userSvc.CreateAsync(dto, CurrentUserId);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message!);
            await LoadDropdowns();
            return View(dto);
        }

        // ── Branch Assignment ───────────────────────────────────────────────
        var branches = dto.BranchIds?.Where(id => id > 0).ToList() ?? new List<int>();

        if (!branches.Any())
        {
          
            var main = (await _branchSvc.GetAllAsync()).FirstOrDefault(b => b.IsMainBranch);
            if (main != null) branches = new List<int> { main.Id };
        }

        if (branches.Any())
        {
            await _branchSvc.AssignUserToBranchesAsync(new UserBranchAssignmentDto
            {
                UserId = result.Data!.Id,
                BranchIds = branches,
                DefaultBranchId = dto.DefaultBranchId > 0
                                    ? dto.DefaultBranchId
                                    : branches.First()
            }, CurrentUserId);
        }

        SetSuccess($"User '{dto.FullName}' সফলভাবে তৈরি হয়েছে।");
        return RedirectToAction(nameof(Index));
    }

    // ── EDIT (GET) ────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit User";
        var user = await _userSvc.GetByIdAsync(id);
        if (user == null) return NotFound();

        var dto = new EditUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Username = user.Username,
            RoleId = user.RoleId,
            IsActive = user.IsActive
        };

        await LoadDropdowns(id);
        return View(dto);
    }

    // ── EDIT (POST) ───────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdowns(id);
            return View(dto);
        }

        var result = await _userSvc.UpdateAsync(id, dto, CurrentUserId);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message!);
            await LoadDropdowns(id);
            return View(dto);
        }

        // ── Branch Assignment Update ─────────────────────────────────────────
        var branches = dto.BranchIds?.Where(bid => bid > 0).ToList() ?? new List<int>();
        if (branches.Any())
        {
            await _branchSvc.AssignUserToBranchesAsync(new UserBranchAssignmentDto
            {
                UserId = id,
                BranchIds = branches,
                DefaultBranchId = dto.DefaultBranchId > 0
                                    ? dto.DefaultBranchId
                                    : branches.First()
            }, CurrentUserId);
        }

        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Index));
    }

    // ── AJAX: Toggle Active/Inactive ─────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        if (id == CurrentUserId)
            return JsonError("নিজের account নিজে deactivate করা যাবে না।");

        var r = await _userSvc.ToggleStatusAsync(id, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    // ── AJAX: Reset Password ─────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return JsonError("Password কমপক্ষে ৬ অক্ষরের হতে হবে।");

        var r = await _userSvc.ResetPasswordAsync(id, newPassword, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    // ── Helper: Dropdowns Load ────────────────────────────────────────────
    private async Task LoadDropdowns(int? editingUserId = null)
    {
        ViewBag.Roles = new SelectList(await _roleSvc.GetAllAsync(), "Id", "Name");
        ViewBag.Branches = (await _branchSvc.GetAllAsync()).Where(b => b.IsActive).ToList();

        if (editingUserId.HasValue)
        {
            var access = await _branchSvc.GetUserAccessAsync(editingUserId.Value, "");
            ViewBag.AssignedBranchIds = access.AccessibleBranches.Select(b => b.Id).ToList();
            ViewBag.DefaultBranchId = access.DefaultBranchId;
        }
        else
        {
            ViewBag.AssignedBranchIds = new List<int>();
            ViewBag.DefaultBranchId = 0;
        }
    }
}