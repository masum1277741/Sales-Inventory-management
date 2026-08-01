namespace ClothingERP.Web.Controllers;

public class SalesController : BaseController
{
    private readonly ISalesService _salesSvc;
    private readonly ICustomerService _customerSvc;
    private readonly IProductService _productSvc;
    private readonly IConfiguration _config;
    private readonly IBundleService _bundleSvc;
    private readonly IExchangeRateService _rateSvc;
    public SalesController(ISalesService salesSvc, ICustomerService customerSvc,
        IProductService productSvc, IConfiguration config, IBundleService bundleSvc, IExchangeRateService rateSvc)
    {
        _salesSvc = salesSvc;
        _customerSvc = customerSvc;
        _productSvc = productSvc;
        _config = config;
        _bundleSvc = bundleSvc;
        _rateSvc = rateSvc;
    }
    // ── All Products for POS Grid ─────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        var variants = await _productSvc.GetAllActiveVariantsAsync();
        return Json(variants.Select(v => new
        {
            variantId = v.Id,
            productName = v.ProductName,
            sku = v.ProductSKU,
            sizeName = v.SizeName,
            colorName = v.ColorName,
            colorHex = v.ColorHex,
            barcode = v.Barcode,
            retailPrice = v.EffectiveRetailPrice,
            costPrice = v.EffectiveCostPrice,
            stock = v.StockQuantity
        }));
    }
    [HttpGet]
    public async Task<IActionResult> POS()
    {
        ViewData["Title"] = "Point of Sale";
        var rates = await _rateSvc.GetCurrentRatesAsync();
        ViewBag.RateBDT = rates.UsdToBdt;
        ViewBag.RateMVR = rates.UsdToMvr;
        ViewBag.RateIsStale = rates.IsStale;
        await LoadRatesAsync();
        return View();
    }
    [HttpGet]
    public async Task<IActionResult> SearchBundles(string keyword)
    {
        var bundles = await _bundleSvc.SearchBundlesAsync(keyword);
        return Json(bundles);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBundles()
    {
        var bundles = await _bundleSvc.SearchBundlesAsync("");
        return Json(bundles);
    }

    private async Task LoadRatesAsync()
    {
        var rates = await _rateSvc.GetCurrentRatesAsync();
        ViewBag.RateBDT = rates.UsdToBdt;
        ViewBag.RateMVR = rates.UsdToMvr;
    }
    // ── Sales History ─────────────────────────────────────────────────────
    public async Task<IActionResult> Index(DateTime? fromDate = null, DateTime? toDate = null)
    {
        ViewData["Title"] = "Sales History";

        var from = fromDate ?? DateTime.Today.AddDays(-30);
        var to = toDate ?? DateTime.Today;
        await LoadRatesAsync();
        var all = await _salesSvc.GetAllAsync();
        var filtered = all.Where(i => i.InvoiceDate.Date >= from && i.InvoiceDate.Date <= to)
                          .OrderByDescending(i => i.InvoiceDate).ToList();

        ViewBag.FromDate = from.ToString("yyyy-MM-dd");
        ViewBag.ToDate = to.ToString("yyyy-MM-dd");
        ViewBag.TodaySales = await _salesSvc.GetTodaySalesAsync();
        ViewBag.TotalSales = filtered.Sum(i => i.TotalAmount);
        ViewBag.TotalDue = filtered.Sum(i => i.DueAmount);
        ViewBag.TotalCount = filtered.Count;

        return View(filtered);
    }

    // ── Invoice Details ───────────────────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Invoice Details";
        await LoadRatesAsync();
        var invoice = await _salesSvc.GetByIdAsync(id);
        if (invoice == null) return NotFound();
        return View(invoice);
    }

    // ── Print (separate printable page) ───────────────────────────────────
    public async Task<IActionResult> Print(int id)
    {
        var invoice = await _salesSvc.GetByIdAsync(id);
        if (invoice == null) return NotFound();
        return View(invoice);
    }

    // ── AJAX: Create Invoice ──────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateSalesInvoiceDto dto)
    {
        if (dto == null || !dto.Items.Any())
            return JsonError("Cart is empty.");

        var result = await _salesSvc.CreateAsync(dto, CurrentUserId);
        return result.Success
            ? JsonSuccess(new
            {
                invoiceId = result.Data!.Id,
                invoiceNumber = result.Data.InvoiceNumber,
                totalAmount = result.Data.TotalAmount,
                totalBDT = result.Data.TotalAmountBDT,
                totalMVR = result.Data.TotalAmountMVR,
                dueAmount = result.Data.DueAmount
            }, result.Message!)
            : JsonError(result.Message!);
    }


    // ── AJAX: Cancel ──────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string reason)
    {
        var r = await _salesSvc.CancelAsync(id, reason, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    // ── AJAX: Hold / Unhold ───────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Hold(int id)
    {
        var r = await _salesSvc.HoldAsync(id, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unhold(int id)
    {
        var r = await _salesSvc.UnholdAsync(id, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    // ── AJAX: Add Payment ─────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPayment(int id, [FromBody] CreateSalesPaymentDto dto)
    {
        var r = await _salesSvc.AddPaymentAsync(id, dto, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    // ── AJAX: Get Held Invoices ───────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetHeldInvoices()
        => Json(await _salesSvc.GetHeldAsync());

    // ── AJAX: Load Held Invoice for POS ──────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetInvoice(int id)
    {
        var inv = await _salesSvc.GetByIdAsync(id);
        return inv == null ? NotFound() : Json(inv);
    }

    // ── AJAX: Search Product by Barcode ───────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> SearchByBarcode(string barcode)
    {
        var variant = await _productSvc.GetVariantByBarcodeAsync(barcode);
        if (variant == null) return Json(null);
        return Json(new
        {
            variantId = variant.Id,
            productName = variant.ProductName,
            sku = variant.ProductSKU,
            sizeName = variant.SizeName,
            colorName = variant.ColorName,
            colorHex = variant.ColorHex,
            barcode = variant.Barcode,
            retailPrice = variant.EffectiveRetailPrice,
            costPrice = variant.EffectiveCostPrice,
            stock = variant.StockQuantity
        });
    }

    // ── AJAX: Search Products ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> SearchProducts(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            return Json(Array.Empty<object>());

        var variants = await _productSvc.SearchVariantsAsync(keyword);
        return Json(variants.Take(15).Select(v => new
        {
            variantId = v.Id,
            productName = v.ProductName,
            sku = v.ProductSKU,
            sizeName = v.SizeName,
            colorName = v.ColorName,
            colorHex = v.ColorHex,
            barcode = v.Barcode,
            retailPrice = v.EffectiveRetailPrice,
            costPrice = v.EffectiveCostPrice,
            stock = v.StockQuantity
        }));
    }

    // ── AJAX: Search Customers ────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> SearchCustomers(string? keyword)
    {
        var all = (await _customerSvc.GetAllAsync())
            .Where(c => c.IsActive)
            .ToList();

       
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return Json(all
                .Take(50)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name ?? "",
                    phoneNumber = c.PhoneNumber ?? "",
                    currentBalance = c.CurrentBalance,
                    groupName = c.GroupName ?? "",
                    loyaltyPoints = c.LoyaltyPoints
                }));
        }

        return Json(all
            .Where(c => c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        (c.PhoneNumber != null &&
                         c.PhoneNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            .Take(20)
            .Select(c => new
            {
                id = c.Id,
                name = c.Name ?? "",
                phoneNumber = c.PhoneNumber ?? "",
                currentBalance = c.CurrentBalance,
                groupName = c.GroupName ?? "",
                loyaltyPoints = c.LoyaltyPoints
            }));
    }
}