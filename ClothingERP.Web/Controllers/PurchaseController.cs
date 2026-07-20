using System.Text.Json;

namespace ClothingERP.Web.Controllers;

public class PurchaseController : BaseController
{
    private readonly IPurchaseService _purchaseSvc;
    private readonly ISupplierService _supplierSvc;
    private readonly IProductService _productSvc;
    private readonly ICurrentBranchProvider _branchProvider;
    private readonly IBranchService _branchSvc;

    public PurchaseController(IPurchaseService purchaseSvc, ISupplierService supplierSvc, IProductService productSvc,
        ICurrentBranchProvider branchProvider, IBranchService branchSvc)
        => (_purchaseSvc, _supplierSvc, _productSvc, _branchProvider, _branchSvc)
            = (purchaseSvc, supplierSvc, productSvc, branchProvider, branchSvc);

    // ── Purchase Orders Index ─────────────────────────────────────────────
    public async Task<IActionResult> Index(DateTime? fromDate = null, DateTime? toDate = null)
    {
        ViewData["Title"] = "Purchase Orders";
        var all = (await _purchaseSvc.GetAllOrdersAsync()).ToList();
        var from = fromDate ?? DateTime.Today.AddDays(-60);
        var to = toDate ?? DateTime.Today;

        var filtered = all.Where(o => o.OrderDate.Date >= from && o.OrderDate.Date <= to)
                          .OrderByDescending(o => o.OrderDate).ToList();

        ViewBag.FromDate = from.ToString("yyyy-MM-dd");
        ViewBag.ToDate = to.ToString("yyyy-MM-dd");
        ViewBag.TotalCount = all.Count;
        ViewBag.DraftCount = all.Count(o => o.Status == "Draft");
        ViewBag.ApprovedCount = all.Count(o => o.Status is "Approved" or "PartiallyReceived");
        ViewBag.ReceivedCount = all.Count(o => o.Status == "FullyReceived");
        ViewBag.TotalValue = all.Sum(o => o.TotalAmount);
        ViewBag.TotalDue = all.Sum(o => o.DueAmount);

        return View(filtered);
    }

    // ── Create PO ─────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "New Purchase Order";
     
        var roleName = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
        var isAdmin = roleName.Equals("Administrator", StringComparison.OrdinalIgnoreCase);

        if (isAdmin)
        {
            ViewBag.Branches = await _branchSvc.GetAllAsync();
        }
        ViewBag.CurrentBranchId = _branchProvider.GetCurrentBranchId();
        ViewBag.Suppliers = await _supplierSvc.GetAllAsync();
        return View(new CreatePurchaseOrderDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePurchaseOrderDto dto, string? itemsJson)
    {
        dto.Items = ParseItems<CreatePurchaseOrderItemDto>(itemsJson);
        if (!dto.Items.Any()) ModelState.AddModelError("", "At least one item is required.");

        if (!ModelState.IsValid)
        {
            ViewBag.Suppliers = await _supplierSvc.GetAllAsync();
            return View(dto);
        }

        var result = await _purchaseSvc.CreateOrderAsync(dto, CurrentUserId);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message!);
            ViewBag.Suppliers = await _supplierSvc.GetAllAsync();
            return View(dto);
        }
        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Details), new { id = result.Data?.Id });
    }

    // ── PO Details ────────────────────────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Purchase Order Details";
        var po = await _purchaseSvc.GetOrderByIdAsync(id);
        if (po == null) return NotFound();

        var allGRNs = await _purchaseSvc.GetAllGRNsAsync();
        ViewBag.GRNs = allGRNs.Where(g => g.PurchaseOrderId == id).ToList();

        return View(po);
    }

    // ── AJAX: Approve / Cancel ────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var r = await _purchaseSvc.ApproveOrderAsync(id, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string reason)
    {
        var r = await _purchaseSvc.CancelOrderAsync(id, reason, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    // ── AJAX: Add Supplier Payment ─────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPayment(int purchaseOrderId, decimal amount, int method, string? reference)
    {
        if (!Enum.IsDefined(typeof(PaymentMethod), method)) method = 1;
        var r = await _purchaseSvc.AddSupplierPaymentAsync(purchaseOrderId, amount, (PaymentMethod)method, reference, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    // ── GRN List ──────────────────────────────────────────────────────────
    public async Task<IActionResult> GRN(DateTime? fromDate = null, DateTime? toDate = null)
    {
        ViewData["Title"] = "Goods Receipt Notes";
        var all = (await _purchaseSvc.GetAllGRNsAsync()).ToList();
        var from = fromDate ?? DateTime.Today.AddDays(-60);
        var to = toDate ?? DateTime.Today;
        var filtered = all.Where(g => g.ReceivedDate.Date >= from && g.ReceivedDate.Date <= to)
                          .OrderByDescending(g => g.ReceivedDate).ToList();

        ViewBag.FromDate = from.ToString("yyyy-MM-dd");
        ViewBag.ToDate = to.ToString("yyyy-MM-dd");
        ViewBag.TotalCount = all.Count;
        ViewBag.TotalValue = all.Sum(g => g.TotalValue);

        return View(filtered);
    }

    // ── Create GRN ────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> CreateGRN(int? purchaseOrderId = null)
    {
        ViewData["Title"] = "Create GRN";
        var orders = await _purchaseSvc.GetAllOrdersAsync();
        ViewBag.PurchaseOrders = orders
            .Where(o => o.Status is "Approved" or "PartiallyReceived")
            .OrderByDescending(o => o.OrderDate).ToList();
        ViewBag.SelectedPOId = purchaseOrderId;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGRN([FromBody] CreateGRNDto dto)
    {
        if (dto == null || !dto.Items.Any()) return JsonError("No items provided.");
        var result = await _purchaseSvc.CreateGRNAsync(dto, CurrentUserId);
        return result.Success
            ? JsonSuccess(new { grnId = result.Data?.Id, grnNumber = result.Data?.GRNNumber }, result.Message!)
            : JsonError(result.Message!);
    }

    // ── GRN Details ───────────────────────────────────────────────────────
    public async Task<IActionResult> GRNDetails(int id)
    {
        ViewData["Title"] = "GRN Details";
        var grn = await _purchaseSvc.GetGRNByIdAsync(id);
        if (grn == null) return NotFound();
        return View(grn);
    }

    // ── AJAX: Get PO items for GRN form ───────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetPOItems(int id)
    {
        var po = await _purchaseSvc.GetOrderByIdAsync(id);
        if (po == null) return NotFound();
        return Json(new
        {
            supplierId = po.SupplierId,
            supplierName = po.SupplierName,
            items = po.Items.Select(i => new
            {
                id = i.Id,
                productVariantId = i.ProductVariantId,
                productName = i.ProductName,
                sizeName = i.SizeName,
                colorName = i.ColorName,
                barcode = i.Barcode,
                orderedQty = i.OrderedQuantity,
                receivedQty = i.ReceivedQuantity,
                remainingQty = i.OrderedQuantity - i.ReceivedQuantity,
                unitCost = i.UnitCost
            }).Where(i => i.remainingQty > 0)
        });
    }

    // ── AJAX: Search Variants ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> SearchVariants(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            return Json(Array.Empty<object>());
        return Json((await _productSvc.SearchVariantsAsync(keyword)).Take(15).Select(v => new
        {
            variantId = v.Id,
            productName = v.ProductName,
            sku = v.ProductSKU,
            sizeName = v.SizeName,
            colorName = v.ColorName,
            colorHex = v.ColorHex,
            barcode = v.Barcode,
            costPrice = v.EffectiveCostPrice
        }));
    }

    // ── Helper ────────────────────────────────────────────────────────────
    private static List<T> ParseItems<T>(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new();
        try
        {
            return JsonSerializer.Deserialize<List<T>>(json,
                  new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }
        catch { return new(); }
    }
}