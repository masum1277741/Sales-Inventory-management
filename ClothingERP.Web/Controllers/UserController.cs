using Microsoft.AspNetCore.Hosting;

namespace ClothingERP.Web.Controllers;

public class UserController : BaseController
{
    private readonly IUserService _users;
    private readonly IRoleService _roles;
    private readonly IWebHostEnvironment _env;

    public UserController(IUserService users, IRoleService roles, IWebHostEnvironment env)
        => (_users, _roles, _env) = (users, roles, env);

    // ── Index ─────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "User Management";
        return View(await _users.GetAllAsync());
    }

    // ── Create ────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Add User";
        await LoadRoles();
        return View(new CreateUserDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        if (!ModelState.IsValid) { await LoadRoles(); return View(dto); }

        var result = await _users.CreateAsync(dto, CurrentUserId);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message!);
            await LoadRoles();
            return View(dto);
        }
        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Index));
    }

    // ── Edit ──────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit User";
        var user = await _users.GetByIdAsync(id);
        if (user == null) return NotFound();

        ViewBag.User = user;
        await LoadRoles();
        return View(new UpdateUserDto
        {
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            RoleId = user.RoleId,
            IsActive = user.IsActive,
            ProfileImagePath = user.ProfileImage
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateUserDto dto, IFormFile? profileImage)
    {
        if (profileImage is { Length: > 0 })
        {
            var dir = Path.Combine(_env.WebRootPath, "uploads", "users");
            Directory.CreateDirectory(dir);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(profileImage.FileName)}";
            await using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
            await profileImage.CopyToAsync(stream);
            dto.ProfileImagePath = $"/uploads/users/{fileName}";
        }

        if (!ModelState.IsValid) { await LoadRoles(); return View(dto); }

        var result = await _users.UpdateAsync(id, dto, CurrentUserId);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message!);
            await LoadRoles();
            return View(dto);
        }
        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Index));
    }

    // ── Ajax Actions ──────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _users.DeleteAsync(id);
        return result.Success ? JsonSuccess(message: result.Message!) : JsonError(result.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _users.ToggleStatusAsync(id, CurrentUserId);
        return result.Success ? JsonSuccess(message: result.Message!) : JsonError(result.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return JsonError("Password must be at least 6 characters.");
        var result = await _users.ResetPasswordAsync(id, newPassword, CurrentUserId);
        return result.Success ? JsonSuccess(message: result.Message!) : JsonError(result.Message!);
    }

    private async Task LoadRoles()
        => ViewBag.Roles = (await _roles.GetAllAsync()).Where(r => r.IsActive);
}