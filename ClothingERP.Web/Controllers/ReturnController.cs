namespace ClothingERP.Web.Controllers;

public class ReturnController : BaseController
{
    private readonly IReturnService _returnSvc;
    private readonly ISalesService _salesSvc;
    private readonly IPurchaseService _purchaseSvc;

    public ReturnController(IReturnService returnSvc, ISalesService salesSvc, IPurchaseService purchaseSvc)
        => (_returnSvc, _salesSvc, _purchaseSvc) = (returnSvc, salesSvc, purchaseSvc);

    // ══ SALES RETURNS ═══════════════════════════════════════════════════
    public async Task<IActionResult> Sales(DateTime? fromDate = null, DateTime? toDate = null)
    {
        ViewData["Title"] = "Sales Returns";
        var all = (await _returnSvc.GetAllSalesReturnsAsync()).ToList();
        var from = fromDate ?? DateTime.Today.AddDays(-30);
        var to = toDate ?? DateTime.Today;

        var filtered = all.Where(r => r.ReturnDate.Date >= from && r.ReturnDate.Date <= to)
                          .OrderByDescending(r => r.ReturnDate).ToList();

        ViewBag.FromDate = from.ToString("yyyy-MM-dd");
        ViewBag.ToDate = to.ToString("yyyy-MM-dd");
        ViewBag.TotalReturns = all.Count;
        ViewBag.TotalAmount = all.Sum(r => r.TotalAmount);
        ViewBag.TotalRefunded = all.Sum(r => r.RefundAmount);
        ViewBag.ReturnCount = all.Count(r => r.ReturnType == "Return");
        ViewBag.ExchangeCount = all.Count(r => r.ReturnType == "Exchange");

        return View(filtered);
    }

    [HttpGet]
    public async Task<IActionResult> CreateSalesReturn(int? invoiceId = null)
    {
        ViewData["Title"] = "Create Sales Return";
        ViewBag.InvoiceId = invoiceId;
        if (invoiceId.HasValue)
        {
            var inv = await _salesSvc.GetByIdAsync(invoiceId.Value);
            ViewBag.Invoice = inv;
        }
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSalesReturn([FromBody] CreateSalesReturnDto dto)
    {
        if (dto == null || !dto.Items.Any())
            return JsonError("No return items provided.");

        var result = await _returnSvc.CreateSalesReturnAsync(dto, CurrentUserId);
        return result.Success
            ? JsonSuccess(new { returnId = result.Data?.Id, returnNumber = result.Data?.ReturnNumber }, result.Message!)
            : JsonError(result.Message!);
    }

    public async Task<IActionResult> SalesReturnDetails(int id)
    {
        ViewData["Title"] = "Return Details";
        var ret = await _returnSvc.GetSalesReturnByIdAsync(id);
        if (ret == null) return NotFound();
        return View(ret);
    }

    // ══ PURCHASE RETURNS ══════════════════════════════════════════════
    public async Task<IActionResult> Purchase(DateTime? fromDate = null, DateTime? toDate = null)
    {
        ViewData["Title"] = "Purchase Returns";
        var all = (await _returnSvc.GetAllPurchaseReturnsAsync()).ToList();
        var from = fromDate ?? DateTime.Today.AddDays(-30);
        var to = toDate ?? DateTime.Today;

        var filtered = all.Where(r => r.ReturnDate.Date >= from && r.ReturnDate.Date <= to)
                          .OrderByDescending(r => r.ReturnDate).ToList();

        ViewBag.FromDate = from.ToString("yyyy-MM-dd");
        ViewBag.ToDate = to.ToString("yyyy-MM-dd");
        ViewBag.TotalReturns = all.Count;
        ViewBag.TotalAmount = all.Sum(r => r.TotalAmount);

        return View(filtered);
    }

    [HttpGet]
    public async Task<IActionResult> CreatePurchaseReturn(int? purchaseOrderId = null)
    {
        ViewData["Title"] = "Create Purchase Return";
        ViewBag.PurchaseOrderId = purchaseOrderId;

        var orders = await _purchaseSvc.GetAllOrdersAsync();
        ViewBag.PurchaseOrders = orders
            .Where(o => o.Status is "PartiallyReceived" or "FullyReceived")
            .OrderByDescending(o => o.OrderDate).ToList();

        if (purchaseOrderId.HasValue)
        {
            var po = await _purchaseSvc.GetOrderByIdAsync(purchaseOrderId.Value);
            ViewBag.PurchaseOrder = po;
        }
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePurchaseReturn([FromBody] CreatePurchaseReturnDto dto)
    {
        if (dto == null || !dto.Items.Any())
            return JsonError("No return items provided.");

        var result = await _returnSvc.CreatePurchaseReturnAsync(dto, CurrentUserId);
        return result.Success
            ? JsonSuccess(new { returnId = result.Data?.Id, returnNumber = result.Data?.ReturnNumber }, result.Message!)
            : JsonError(result.Message!);
    }

    public async Task<IActionResult> PurchaseReturnDetails(int id)
    {
        ViewData["Title"] = "Purchase Return Details";
        var ret = await _returnSvc.GetPurchaseReturnByIdAsync(id);
        if (ret == null) return NotFound();
        return View(ret);
    }

    // ══ AJAX Helpers ══════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> SearchInvoices(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2) return Json(Array.Empty<object>());
        var invoices = (await _salesSvc.GetAllAsync())
            .Where(i => i.Status != "Cancelled" && !i.IsHold &&
                       (i.InvoiceNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        i.CustomerName.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            .Take(10)
            .Select(i => new {
                i.Id,
                i.InvoiceNumber,
                i.CustomerName,
                i.TotalAmount,
                date = i.InvoiceDate.ToString("dd MMM yyyy")
            });
        return Json(invoices);
    }

    [HttpGet]
    public async Task<IActionResult> GetInvoiceForReturn(int id)
    {
        var inv = await _salesSvc.GetByIdAsync(id);
        if (inv == null) return NotFound();
        return Json(new
        {
            id = inv.Id,
            invoiceNumber = inv.InvoiceNumber,
            customerId = inv.CustomerId,
            customerName = inv.CustomerName,
            totalAmount = inv.TotalAmount,
            items = inv.Items.Select(i => new
            {
                productVariantId = i.ProductVariantId,
                productName = i.ProductName,
                sizeName = i.SizeName,
                colorName = i.ColorName,
                colorHex = "#888",
                barcode = i.Barcode,
                quantity = i.Quantity,
                unitPrice = i.UnitPrice,
                totalAmount = i.TotalAmount
            })
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetPOForReturn(int id)
    {
        var po = await _purchaseSvc.GetOrderByIdAsync(id);
        if (po == null) return NotFound();
        return Json(new
        {
            id = po.Id,
            poNumber = po.PONumber,
            supplierId = po.SupplierId,
            supplierName = po.SupplierName,
            items = po.Items
                .Where(i => i.ReceivedQuantity > 0)
                .Select(i => new
                {
                    productVariantId = i.ProductVariantId,
                    productName = i.ProductName,
                    sizeName = i.SizeName,
                    colorName = i.ColorName,
                    barcode = i.Barcode,
                    receivedQty = i.ReceivedQuantity,
                    unitCost = i.UnitCost
                })
        });
    }
}