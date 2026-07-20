namespace ClothingERP.Web.Controllers;

public class ReorderController : BaseController
{
    private readonly IReorderService _reorderSvc;
    private readonly ISupplierService _supplierSvc;

    public ReorderController(IReorderService reorderSvc, ISupplierService supplierSvc)
        => (_reorderSvc, _supplierSvc) = (reorderSvc, supplierSvc);

 
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Smart Reorder Suggestions";
        var suggestions = await _reorderSvc.GetSuggestionsAsync();
        var summary = await _reorderSvc.GetSummaryAsync();

        ViewBag.Summary = summary;
        ViewBag.Suppliers = await _supplierSvc.GetAllAsync();

        return View(suggestions);
    }

    // ── Settings ──────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        ViewData["Title"] = "Reorder Settings";
        var settings = await _reorderSvc.GetSettingsAsync();
        return View(settings);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(UpdateReorderSettingsDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var r = await _reorderSvc.UpdateSettingsAsync(dto, CurrentUserId);
        SetSuccess(r.Message!);
        return RedirectToAction(nameof(Settings));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GeneratePO([FromBody] GeneratePOFromSuggestionsDto dto)
    {
        var result = await _reorderSvc.GeneratePurchaseOrderAsync(dto, CurrentUserId);
        if (!result.Success) return JsonError(result.Message!);
        return JsonSuccess(new { poId = result.Data }, result.Message!);
    }

    [HttpGet]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _reorderSvc.GetSummaryAsync();
        return Json(summary);
    }
}