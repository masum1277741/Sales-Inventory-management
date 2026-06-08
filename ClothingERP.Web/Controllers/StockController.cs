namespace ClothingERP.Web.Controllers;

public class StockController : BaseController
{
    private readonly IStockService _stockSvc;
    private readonly ICategoryService _catSvc;
    private readonly IProductService _productSvc;

    public StockController(IStockService stockSvc, ICategoryService catSvc, IProductService productSvc)
        => (_stockSvc, _catSvc, _productSvc) = (stockSvc, catSvc, productSvc);

    // ── Index ─────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(string? filter = null, int? categoryId = null)
    {
        ViewData["Title"] = "Stock Management";

        var all = (await _stockSvc.GetAllAsync()).ToList();
        var categories = (await _catSvc.GetCategoriesAsync()).ToList();

        IEnumerable<StockListDto> stock = all;

        if (filter == "low") stock = all.Where(s => s.Status == "Low Stock");
        else if (filter == "out") stock = all.Where(s => s.Status == "Out of Stock");
        else if (filter == "in") stock = all.Where(s => s.Status == "In Stock");

        if (categoryId.HasValue)
        {
            var catName = categories.FirstOrDefault(c => c.Id == categoryId.Value)?.Name;
            if (catName != null) stock = stock.Where(s => s.CategoryName == catName);
        }

        ViewBag.Filter = filter;
        ViewBag.CategoryId = categoryId;
        ViewBag.Categories = categories;
        ViewBag.TotalCount = all.Count;
        ViewBag.InStock = all.Count(s => s.Status == "In Stock");
        ViewBag.LowStock = all.Count(s => s.Status == "Low Stock");
        ViewBag.OutOfStock = all.Count(s => s.Status == "Out of Stock");
        ViewBag.TotalValue = all.Sum(s => s.StockValue);

        return View(stock);
    }

    // ── Stock Movement History ────────────────────────────────────────────
    public async Task<IActionResult> Movements(int id)
    {
        ViewData["Title"] = "Stock Movement History";
        var stock = await _stockSvc.GetByVariantIdAsync(id);
        if (stock == null) return NotFound();
        return View(stock);
    }

    // ── Adjustment Page ───────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Adjustment()
    {
        ViewData["Title"] = "Stock Adjustment";
        return View();
    }

    // ── AJAX: Submit Adjustment ───────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Adjust(StockAdjustmentDto dto)
    {
        if (!ModelState.IsValid)
            return JsonError(string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

        var result = await _stockSvc.AdjustStockAsync(dto, CurrentUserId);
        return result.Success
            ? JsonSuccess(new { newQty = dto.NewQuantity }, result.Message!)
            : JsonError(result.Message!);
    }

    // ── AJAX: Search Variant by Barcode ───────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> SearchByBarcode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return Json(null);

        var variant = await _productSvc.GetVariantByBarcodeAsync(barcode);
        if (variant == null) return Json(null);

        var stock = await _stockSvc.GetByVariantIdAsync(variant.Id);
        return Json(new
        {
            variantId = variant.Id,
            productName = variant.ProductName,
            sku = variant.ProductSKU,
            sizeName = variant.SizeName,
            colorName = variant.ColorName,
            colorHex = variant.ColorHex,
            barcode = variant.Barcode,
            currentQty = stock?.Quantity ?? 0,
            retailPrice = variant.EffectiveRetailPrice,
            costPrice = variant.EffectiveCostPrice
        });
    }

    // ── AJAX: Search Variants by Keyword ──────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> SearchVariants(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            return Json(Array.Empty<object>());

        var variants = await _productSvc.SearchVariantsAsync(keyword);
        return Json(variants.Take(20).Select(v => new
        {
            v.Id,
            v.ProductName,
            v.ProductSKU,
            v.SizeName,
            v.ColorName,
            v.ColorHex,
            v.Barcode,
            v.StockQuantity,
            v.EffectiveCostPrice
        }));
    }

    // ── AJAX: Summary Stats ───────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Summary()
    {
        var all = (await _stockSvc.GetAllAsync()).ToList();
        return Json(new
        {
            total = all.Count,
            inStock = all.Count(s => s.Status == "In Stock"),
            lowStock = all.Count(s => s.Status == "Low Stock"),
            outOfStock = all.Count(s => s.Status == "Out of Stock"),
            totalValue = all.Sum(s => s.StockValue)
        });
    }
}