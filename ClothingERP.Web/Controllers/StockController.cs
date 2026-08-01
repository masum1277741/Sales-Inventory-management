namespace ClothingERP.Web.Controllers;

public class StockController : BaseController
{
    private readonly IStockService _stockSvc;
    private readonly ICategoryService _catSvc;
    private readonly IProductService _productSvc;
    private readonly ICurrentBranchProvider _branchProvider;
    private readonly IBranchService _branchSvc;
    private readonly IExchangeRateService _rateSvc;

    public StockController(IStockService stockSvc, ICategoryService catSvc, IProductService productSvc,
                            ICurrentBranchProvider branchProvider, IBranchService branchSvc,
                            IExchangeRateService rateSvc)
        => (_stockSvc, _catSvc, _productSvc, _branchProvider, _branchSvc, _rateSvc)
            = (stockSvc, catSvc, productSvc, branchProvider, branchSvc, rateSvc);

    private bool IsAdmin =>
        (User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "")
            .Equals("Administrator", StringComparison.OrdinalIgnoreCase);

    // ── Index ─────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(string? filter = null, int? categoryId = null,
                                            int? branchId = null, bool allBranches = false)
    {
        ViewData["Title"] = "Stock Management";

        var myBranchId = _branchProvider.GetCurrentBranchId();
        int? effectiveBranchId = IsAdmin && allBranches ? null
                                : IsAdmin && branchId.HasValue ? branchId
                                : myBranchId;

        var all = (await _stockSvc.GetAllAsync(effectiveBranchId)).ToList();
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

        var rates = await _rateSvc.GetCurrentRatesAsync();

        ViewBag.Filter = filter;
        ViewBag.CategoryId = categoryId;
        ViewBag.Categories = categories;
        ViewBag.TotalCount = all.Count;
        ViewBag.InStock = all.Count(s => s.Status == "In Stock");
        ViewBag.LowStock = all.Count(s => s.Status == "Low Stock");
        ViewBag.OutOfStock = all.Count(s => s.Status == "Out of Stock");
        ViewBag.TotalValue = all.Sum(s => s.StockValue);
        ViewBag.RateBDT = rates.UsdToBdt;

        ViewBag.IsAdmin = IsAdmin;
        ViewBag.Branches = IsAdmin ? await _branchSvc.GetAllAsync() : null;
        ViewBag.SelectedBranch = effectiveBranchId;
        ViewBag.ShowAllBranches = allBranches && IsAdmin;

        return View(stock);
    }

    // ── Stock Movement History ────────────────────────────────────────────
    public async Task<IActionResult> Movements(int id, int? branchId = null)
    {
        ViewData["Title"] = "Stock Movement History";
        var effectiveBranchId = IsAdmin && branchId.HasValue ? branchId : _branchProvider.GetCurrentBranchId();
        var stock = await _stockSvc.GetByVariantIdAsync(id, effectiveBranchId);
        if (stock == null) return NotFound();
        return View(stock);
    }

    // ── Adjustment Page ───────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Adjustment()
    {
        ViewData["Title"] = "Stock Adjustment";
        ViewBag.IsAdmin = IsAdmin;
        ViewBag.CurrentBranchId = _branchProvider.GetCurrentBranchId();
        ViewBag.Branches = IsAdmin ? await _branchSvc.GetAllAsync() : null;
        return View();
    }

    // ── AJAX: Submit Adjustment ───────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Adjust(StockAdjustmentDto dto)
    {
    
        if (!IsAdmin || dto.BranchId <= 0)
            dto.BranchId = _branchProvider.GetCurrentBranchId();

        if (!ModelState.IsValid)
            return JsonError(string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

        var result = await _stockSvc.AdjustStockAsync(dto, CurrentUserId);
        return result.Success
            ? JsonSuccess(new { newQty = dto.NewQuantity }, result.Message!)
            : JsonError(result.Message!);
    }


    [HttpGet]
    public async Task<IActionResult> SearchByBarcode(string barcode, int? branchId = null)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return Json(null);

        var effectiveBranchId = IsAdmin && branchId.HasValue ? branchId : _branchProvider.GetCurrentBranchId();

        var variant = await _productSvc.GetVariantByBarcodeAsync(barcode);
        if (variant == null) return Json(null);

        var qty = await _stockSvc.GetVariantQuantityAsync(variant.Id, effectiveBranchId);
        return Json(new
        {
            variantId = variant.Id,
            productName = variant.ProductName,
            sku = variant.ProductSKU,
            sizeName = variant.SizeName,
            colorName = variant.ColorName,
            colorHex = variant.ColorHex,
            barcode = variant.Barcode,
            currentQty = qty,
            retailPrice = variant.EffectiveRetailPrice,
            costPrice = variant.EffectiveCostPrice
        });
    }

    [HttpGet]
    public async Task<IActionResult> SearchVariants(string keyword, int? branchId = null)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            return Json(Array.Empty<object>());

        var effectiveBranchId = IsAdmin && branchId.HasValue ? branchId : _branchProvider.GetCurrentBranchId();

        var variants = (await _productSvc.SearchVariantsAsync(keyword)).Take(20).ToList();

        var result = new List<object>();
        foreach (var v in variants)
        {
            var qty = await _stockSvc.GetVariantQuantityAsync(v.Id, effectiveBranchId);
            result.Add(new
            {
                v.Id,
                v.ProductName,
                v.ProductSKU,
                v.SizeName,
                v.ColorName,
                v.ColorHex,
                v.Barcode,
                StockQuantity = qty,
                v.EffectiveCostPrice
            });
        }
        return Json(result);
    }

    // ── AJAX: Summary Stats ───────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Summary(int? branchId = null)
    {
        var effectiveBranchId = IsAdmin && branchId.HasValue ? branchId : _branchProvider.GetCurrentBranchId();
        var all = (await _stockSvc.GetAllAsync(effectiveBranchId)).ToList();
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