namespace ClothingERP.Web.Controllers;

[Authorize]
public abstract class BaseController : Controller
{
    protected int CurrentUserId =>
        int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    protected string CurrentUsername =>
        User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

    protected string CurrentUserFullName =>
        User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Unknown";

    protected int CurrentRoleId =>
        int.TryParse(User.FindFirst("RoleId")?.Value, out var id) ? id : 0;

    protected string CurrentIPAddress =>
        HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

    protected void SetSuccess(string message) => TempData["Success"] = message;
    protected void SetError(string message) => TempData["Error"] = message;
    protected void SetWarning(string message) => TempData["Warning"] = message;

    protected IActionResult HandleResult(ServiceResult result, string? redirectAction = null, string? redirectController = null)
    {
        if (result.Success)
        {
            SetSuccess(result.Message ?? "Operation successful.");
            if (redirectAction != null)
                return RedirectToAction(redirectAction, redirectController);
        }
        else
        {
            SetError(result.Message ?? "Operation failed.");
        }
        return RedirectToAction("Index");
    }

    protected JsonResult JsonSuccess(object? data = null, string message = "Success")
        => Json(new { success = true, message, data });

    protected JsonResult JsonError(string message = "Error occurred")
        => Json(new { success = false, message });
}