using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;

namespace ClothingERP.Web.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    // ── Root URL Redirect ─────────────────────────────────────────────────
    public IActionResult Index()
    {
        return User.Identity?.IsAuthenticated == true
            ? RedirectToAction("Index", "Dashboard")
            : RedirectToAction("Login", "Auth");
    }

    // ── Global Error Handler ──────────────────────────────────────────────
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        ViewBag.ErrorMessage = feature?.Error?.Message ?? "An unexpected error occurred.";
        ViewBag.ErrorPath = feature?.Path ?? "Unknown path";
        return View();
    }

    // ── 404 Not Found ─────────────────────────────────────────────────────
    public IActionResult NotFound404()
    {
        Response.StatusCode = 404;
        return View("NotFound");
    }
}