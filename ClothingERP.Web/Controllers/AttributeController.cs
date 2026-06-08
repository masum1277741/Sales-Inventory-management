namespace ClothingERP.Web.Controllers;

public class AttributeController : BaseController
{
    private readonly IProductAttributeService _attrSvc;

    public AttributeController(IProductAttributeService attrSvc) => _attrSvc = attrSvc;

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Sizes & Colors";
        ViewBag.Sizes = await _attrSvc.GetSizesAsync();
        ViewBag.Colors = await _attrSvc.GetColorsAsync();
        return View();
    }

    // ── Sizes ─────────────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSize(CreateSizeDto dto)
    {
        var r = await _attrSvc.CreateSizeAsync(dto, CurrentUserId);
        return r.Success ? JsonSuccess(r.Data, r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSize(int id, CreateSizeDto dto)
    {
        var r = await _attrSvc.UpdateSizeAsync(id, dto, CurrentUserId);
        return r.Success ? JsonSuccess(r.Data, r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSize(int id)
    {
        var r = await _attrSvc.DeleteSizeAsync(id);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    // ── Colors ────────────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateColor(CreateColorDto dto)
    {
        var r = await _attrSvc.CreateColorAsync(dto, CurrentUserId);
        return r.Success ? JsonSuccess(r.Data, r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditColor(int id, CreateColorDto dto)
    {
        var r = await _attrSvc.UpdateColorAsync(id, dto, CurrentUserId);
        return r.Success ? JsonSuccess(r.Data, r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteColor(int id)
    {
        var r = await _attrSvc.DeleteColorAsync(id);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }
}