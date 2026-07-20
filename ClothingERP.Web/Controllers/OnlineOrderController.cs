namespace ClothingERP.Web.Controllers;

public class OnlineOrderController : BaseController
{
    private readonly IOnlineOrderService _orderSvc;
    private readonly IStorefrontService _storefrontSvc;

    public OnlineOrderController(IOnlineOrderService orderSvc, IStorefrontService storefrontSvc)
        => (_orderSvc, _storefrontSvc) = (orderSvc, storefrontSvc);

    public async Task<IActionResult> Index(string? status)
    {
        ViewData["Title"] = "Online Orders";
        var orders = await _orderSvc.GetAllAsync(status);
        ViewBag.StatusFilter = status;
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Order Details";
        var order = await _orderSvc.GetByIdAsync(id);
        if (order == null) return NotFound();
        return View(order);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(UpdateOrderStatusDto dto)
    {
        var r = await _orderSvc.UpdateStatusAsync(dto, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    // ── Storefront Settings ───────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        ViewData["Title"] = "Storefront Settings";
        var settings = await _storefrontSvc.GetSettingsAsync();
        return View(settings);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(StorefrontSettingsDto dto)
    {
        var r = await _storefrontSvc.UpdateSettingsAsync(dto, CurrentUserId);
        SetSuccess(r.Message!);
        return RedirectToAction(nameof(Settings));
    }
}