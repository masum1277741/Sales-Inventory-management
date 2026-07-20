using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace ClothingERP.Web.Controllers;

[AllowAnonymous]
public class ShopAccountController : Controller
{
    private readonly ICustomerAuthService _authSvc;

    public ShopAccountController(ICustomerAuthService authSvc) => _authSvc = authSvc;

    [HttpGet] public IActionResult Login() => View(new CustomerLoginDto());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(CustomerLoginDto dto, string? returnUrl = null)
    {
        var result = await _authSvc.LoginAsync(dto);
        if (!result.Success) { ModelState.AddModelError("", result.Message!); return View(dto); }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.Data!.Id.ToString()),
            new(ClaimTypes.Name, result.Data.Name)
        };
        var identity = new ClaimsIdentity(claims, "CustomerAuth");
        await HttpContext.SignInAsync("CustomerAuth", new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30) });

        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Shop");
    }

    [HttpGet] public IActionResult Register() => View(new CustomerRegisterDto());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(CustomerRegisterDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await _authSvc.RegisterAsync(dto);
        if (!result.Success) { ModelState.AddModelError("", result.Message!); return View(dto); }

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, result.Data.ToString()), new(ClaimTypes.Name, dto.Name) };
        var identity = new ClaimsIdentity(claims, "CustomerAuth");
        await HttpContext.SignInAsync("CustomerAuth", new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });

        TempData["success"] = "Account তৈরি হয়েছে — স্বাগতম!";
        return RedirectToAction("Index", "Shop");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("CustomerAuth");
        return RedirectToAction("Index", "Shop");
    }

    [Authorize(AuthenticationSchemes = "CustomerAuth")]
    public async Task<IActionResult> MyOrders()
    {
        var customerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var orders = await _authSvc.GetMyOrdersAsync(customerId);
        return View(orders);
    }
}