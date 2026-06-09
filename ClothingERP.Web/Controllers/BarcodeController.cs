using ClothingERP.Web.Models;

namespace ClothingERP.Web.Controllers;

public class BarcodeController : BaseController
{
    private readonly IProductService _productSvc;

    public BarcodeController(IProductService productSvc) => _productSvc = productSvc;

    // ── Index ─────────────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Barcode Print";
        return View();
    }

    // ── Print Labels ──────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult PrintLabels(
        [FromForm] List<string> productNames,
        [FromForm] List<string> sizeNames,
        [FromForm] List<string> colorNames,
        [FromForm] List<string> barcodes,
        [FromForm] List<decimal> retailPrices,
        [FromForm] List<string> skus,
        [FromForm] List<int> printQtys)
    {
        if (barcodes == null || !barcodes.Any())
        {
            SetError("Please add at least one item to print.");
            return RedirectToAction(nameof(Index));
        }

        var labels = new List<BarcodeLabelViewModel>();
        for (int i = 0; i < barcodes.Count; i++)
        {
            var qty = printQtys?.ElementAtOrDefault(i) ?? 1;
            qty = Math.Max(1, Math.Min(qty, 50));
            for (int q = 0; q < qty; q++)
            {
                labels.Add(new BarcodeLabelViewModel
                {
                    ProductName = productNames?.ElementAtOrDefault(i) ?? "",
                    SizeName = sizeNames?.ElementAtOrDefault(i) ?? "",
                    ColorName = colorNames?.ElementAtOrDefault(i) ?? "",
                    Barcode = barcodes[i],
                    RetailPrice = retailPrices?.ElementAtOrDefault(i) ?? 0,
                    SKU = skus?.ElementAtOrDefault(i),
                    PrintQty = qty
                });
            }
        }

        if (!labels.Any()) { SetError("No labels to print."); return RedirectToAction(nameof(Index)); }
        return View("Print", labels);
    }

    // ── AJAX: Search variants ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> SearchVariants(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            return Json(Array.Empty<object>());

        var variants = await _productSvc.SearchVariantsAsync(keyword);
        return Json(variants.Take(20).Select(v => new
        {
            variantId = v.Id,
            productName = v.ProductName,
            sku = v.ProductSKU,
            sizeName = v.SizeName,
            colorName = v.ColorName,
            colorHex = v.ColorHex,
            barcode = v.Barcode,
            retailPrice = v.EffectiveRetailPrice,
            stock = v.StockQuantity
        }));
    }

    // ── AJAX: Lookup by barcode ───────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> LookupBarcode(string barcode)
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
            stock = variant.StockQuantity
        });
    }
}