namespace ClothingERP.Web.Controllers;

public class ExchangeRateController : BaseController
{
    private readonly IExchangeRateService _rateSvc;

    public ExchangeRateController(IExchangeRateService rateSvc) => _rateSvc = rateSvc;

    // ── Settings + Current Rates Page ─────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Exchange Rate Settings";
        ViewBag.CurrentRates = await _rateSvc.GetCurrentRatesAsync();
        ViewBag.Settings = await _rateSvc.GetSettingsAsync();
        var history = await _rateSvc.GetHistoryAsync(20);
        return View(history);
    }

    // ── AJAX: Manual Refresh Button ───────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RefreshNow()
    {
        var result = await _rateSvc.RefreshFromApiAsync();
        return Json(result);
    }

    // ── AJAX: Manual Override ────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetManualRate(ManualRateOverrideDto dto)
    {
        var r = await _rateSvc.SetManualRateAsync(dto, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    // ── AJAX: Settings Update ────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSettings(UpdateExchangeRateSettingsDto dto)
    {
        var r = await _rateSvc.UpdateSettingsAsync(dto, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentRates()
    {
        var rates = await _rateSvc.GetCurrentRatesAsync();
        return Json(rates);
    }
}