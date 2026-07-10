namespace ClothingERP.Web.Controllers;

public class BundleController : BaseController
{
    private readonly IBundleService _bundleSvc;
    private readonly IProductService _productSvc;

    public BundleController(IBundleService bundleSvc, IProductService productSvc)
        => (_bundleSvc, _productSvc) = (bundleSvc, productSvc);

    // ── Index ─────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Combo & Bundle Offers";
        var bundles = await _bundleSvc.GetAllAsync();
        return View(bundles);
    }

    // ── Create ────────────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Bundle Offer";
        return View(new CreateProductBundleDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductBundleDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var result = await _bundleSvc.CreateAsync(dto, CurrentUserId);
        if (!result.Success) { ModelState.AddModelError("", result.Message!); return View(dto); }
        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Index));
    }

    // ── Edit ──────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Bundle Offer";
        var bundle = await _bundleSvc.GetByIdAsync(id);
        if (bundle == null) return NotFound();
        return View(bundle);
    }

    // ── AJAX: Bundle Details ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var bundle = await _bundleSvc.GetByIdAsync(id);
        return bundle == null ? NotFound() : Json(bundle);
    }

    // ── AJAX: Delete / Toggle ─────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _bundleSvc.DeleteAsync(id);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var r = await _bundleSvc.ToggleStatusAsync(id, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    // ── AJAX: Variant Search (item picker এর জন্য) ────────────────────────
    [HttpGet]
    public async Task<IActionResult> SearchVariants(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            return Json(Array.Empty<object>());
        var variants = await _productSvc.SearchVariantsAsync(keyword);
        return Json(variants.Take(15).Select(v => new
        {
            variantId = v.Id,
            productName = v.ProductName,
            sizeName = v.SizeName,
            colorName = v.ColorName,
            barcode = v.Barcode,
            retailPrice = v.EffectiveRetailPrice,
            stock = v.StockQuantity
        }));
    }
}