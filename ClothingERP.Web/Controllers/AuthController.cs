using Microsoft.AspNetCore.Authentication.Cookies;

namespace ClothingERP.Web.Controllers;

[AllowAnonymous]
public class AuthController : Controller
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    // ── GET Login ─────────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginDto());
    }

    // ── POST Login ────────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(dto);

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var ua = HttpContext.Request.Headers["User-Agent"].ToString();

        var result = await _auth.LoginAsync(dto, ip, ua);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message ?? "Invalid credentials.");
            return View(dto);
        }

        var user = result.Data!;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.Username),
            new(ClaimTypes.GivenName,      user.FullName),
            new("RoleId",                  user.RoleId.ToString()),
            new(ClaimTypes.Role,           user.RoleName)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = dto.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(dto.RememberMe ? 720 : 8)
            });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    // ── Logout ────────────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        await _auth.LogoutAsync(userId, ip);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Login");
    }

    // ── GET Logout (via link) ──────────────────────────────────────────────
    [HttpGet, Authorize]
    public async Task<IActionResult> LogoutGet()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // ── Change Password ───────────────────────────────────────────────────
    [HttpGet, Authorize]
    public IActionResult ChangePassword() => View(new ChangePasswordDto());

    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _auth.ChangePasswordAsync(userId, dto);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message ?? "Failed.");
            return View(dto);
        }

        TempData["Success"] = "Password changed successfully!";
        return RedirectToAction("Index", "Dashboard");
    }

    // ── Profile ───────────────────────────────────────────────────────────
    [HttpGet, Authorize]
    public async Task<IActionResult> Profile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _auth.GetCurrentUserAsync(userId);
        if (user == null) return RedirectToAction("Login");
        ViewData["Title"] = "My Profile";
        return View(user);
    }

    // ── Access Denied ─────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Access Denied";
        return View();
    }
}