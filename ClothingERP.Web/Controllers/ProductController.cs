using System.Text.Json;

namespace ClothingERP.Web.Controllers;

public class ProductController : BaseController
{
    private readonly IProductService _products;
    private readonly ICategoryService _cats;
    private readonly IBrandService _brands;
    private readonly IProductAttributeService _attrs;
    private readonly IWebHostEnvironment _env;

    public ProductController(IProductService products, ICategoryService cats,
        IBrandService brands, IProductAttributeService attrs, IWebHostEnvironment env)
    { _products = products; _cats = cats; _brands = brands; _attrs = attrs; _env = env; }

    // ── Index ─────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Product Management";
        ViewBag.Categories = await _cats.GetCategoriesAsync();
        return View(await _products.GetAllAsync());
    }

    // ── Create ────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Add Product";
        await LoadLookups();
        return View(new CreateProductDto());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductDto dto, IFormFile? productImage, string? variantsJson)
    {
        dto.Variants = ParseVariants(variantsJson);

        if (productImage is { Length: > 0 })
            dto.ImagePath = await SaveFile(productImage, "products");

        if (!ModelState.IsValid) { await LoadLookups(dto.CategoryId); return View(dto); }

        var result = await _products.CreateAsync(dto, CurrentUserId);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message!);
            await LoadLookups(dto.CategoryId);
            return View(dto);
        }
        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Details), new { id = result.Data?.Id });
    }

    // ── Edit ──────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Product";
        var p = await _products.GetByIdAsync(id);
        if (p == null) return NotFound();

        ViewBag.Product = p;
        await LoadLookups(p.CategoryId);
        return View(new UpdateProductDto
        {
            Name = p.Name,
            Description = p.Description,
            CategoryId = p.CategoryId,
            SubCategoryId = p.SubCategoryId,
            BrandId = p.BrandId,
            CostPrice = p.CostPrice,
            RetailPrice = p.RetailPrice,
            WholesalePrice = p.WholesalePrice,
            SpecialPrice = p.SpecialPrice,
            TaxRate = p.TaxRate,
            ReorderPoint = p.ReorderPoint,
            IsActive = p.IsActive,
            ImagePath = p.ImagePath
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateProductDto dto, IFormFile? productImage)
    {
        if (productImage is { Length: > 0 })
            dto.ImagePath = await SaveFile(productImage, "products");

        if (!ModelState.IsValid) { await LoadLookups(dto.CategoryId); return View(dto); }

        var result = await _products.UpdateAsync(id, dto, CurrentUserId);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Message!);
            await LoadLookups(dto.CategoryId);
            return View(dto);
        }
        SetSuccess(result.Message!);
        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Details ───────────────────────────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Product Details";
        var product = await _products.GetByIdAsync(id);
        if (product == null) return NotFound();
        ViewBag.Sizes = await _attrs.GetSizesAsync();
        ViewBag.Colors = await _attrs.GetColorsAsync();
        return View(product);
    }

    // ── AJAX Actions ──────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _products.DeleteAsync(id);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var r = await _products.ToggleStatusAsync(id, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVariant(int productId, int sizeId, int colorId,
        decimal? costPriceOverride, decimal? retailPriceOverride)
    {
        var dto = new CreateProductVariantDto
        {
            SizeId = sizeId,
            ColorId = colorId,
            CostPriceOverride = costPriceOverride,
            RetailPriceOverride = retailPriceOverride
        };
        var r = await _products.AddVariantAsync(productId, dto, CurrentUserId);
        return r.Success ? JsonSuccess(r.Data, r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVariant(int variantId)
    {
        var r = await _products.DeleteVariantAsync(variantId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpGet]
    public async Task<IActionResult> GetSubCategories(int categoryId)
        => Json((await _cats.GetSubCategoriesAsync(categoryId)).Select(s => new { s.Id, s.Name }));

    [HttpGet]
    public async Task<IActionResult> SearchByBarcode(string barcode)
        => Json(await _products.GetVariantByBarcodeAsync(barcode));

    [HttpGet]
    public async Task<IActionResult> SearchVariants(string keyword)
        => Json(await _products.SearchVariantsAsync(keyword));

    // ── Helpers ───────────────────────────────────────────────────────────
    private async Task LoadLookups(int? categoryId = null)
    {
        ViewBag.Categories = await _cats.GetCategoriesAsync();
        ViewBag.SubCategories = categoryId.HasValue
            ? await _cats.GetSubCategoriesAsync(categoryId.Value)
            : await _cats.GetSubCategoriesAsync();
        ViewBag.Brands = await _brands.GetAllAsync();
        ViewBag.Sizes = await _attrs.GetSizesAsync();
        ViewBag.Colors = await _attrs.GetColorsAsync();
    }

    private async Task<string> SaveFile(IFormFile file, string folder)
    {
        var dir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(dir);
        var name = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        await using var s = new FileStream(Path.Combine(dir, name), FileMode.Create);
        await file.CopyToAsync(s);
        return $"/uploads/{folder}/{name}";
    }

    private static List<CreateProductVariantDto> ParseVariants(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new();
        try
        {
            return JsonSerializer.Deserialize<List<CreateProductVariantDto>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }
        catch { return new(); }
    }
}