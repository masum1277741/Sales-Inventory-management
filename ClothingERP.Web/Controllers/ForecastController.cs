namespace ClothingERP.Web.Controllers;

public class ForecastController : BaseController
{
    private readonly IDemandForecastService _forecastSvc;

    public ForecastController(IDemandForecastService forecastSvc) => _forecastSvc = forecastSvc;


    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "AI Demand Forecasting";
        var summary = await _forecastSvc.GetSummaryAsync(15);
        return View(summary);
    }


    public async Task<IActionResult> Details(int variantId)
    {
        ViewData["Title"] = "Demand Forecast Details";
        var forecast = await _forecastSvc.ForecastForVariantAsync(variantId);
        return View(forecast);
    }

    // ── Settings ──────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        ViewData["Title"] = "Forecast Settings";
        var settings = await _forecastSvc.GetSettingsAsync();
        return View(settings);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(UpdateForecastSettingsDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var r = await _forecastSvc.UpdateSettingsAsync(dto, CurrentUserId);
        SetSuccess(r.Message!);
        return RedirectToAction(nameof(Settings));
    }

    // ── AJAX: Product Search ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> SearchProducts(string keyword)
    {
        var results = await _forecastSvc.SearchForecastsAsync(keyword);
        return Json(results);
    }


    [HttpGet]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _forecastSvc.GetSummaryAsync(5);
        return Json(summary);
    }
}