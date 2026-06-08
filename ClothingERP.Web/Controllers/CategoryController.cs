using System.Text.Json;

namespace ClothingERP.Web.Controllers;

public class CategoryController : BaseController
{
    private readonly ICategoryService _catSvc;
    private readonly IWebHostEnvironment _env;

    public CategoryController(ICategoryService catSvc, IWebHostEnvironment env)
        => (_catSvc, _env) = (catSvc, env);

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Category Management";
        ViewBag.Categories = await _catSvc.GetCategoriesAsync();
        ViewBag.SubCategories = await _catSvc.GetSubCategoriesAsync();
        return View();
    }

    // ── Category AJAX ─────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetCategory(int id)
    {
        var c = await _catSvc.GetCategoryByIdAsync(id);
        return c == null ? NotFound() : Json(c);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(CreateCategoryDto dto, IFormFile? image)
    {
        if (image is { Length: > 0 }) dto.ImagePath = await SaveFile(image, "categories");
        var result = await _catSvc.CreateCategoryAsync(dto, CurrentUserId);
        return result.Success ? JsonSuccess(result.Data, result.Message!) : JsonError(result.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, CreateCategoryDto dto, IFormFile? image)
    {
        if (image is { Length: > 0 }) dto.ImagePath = await SaveFile(image, "categories");
        var result = await _catSvc.UpdateCategoryAsync(id, dto, CurrentUserId);
        return result.Success ? JsonSuccess(result.Data, result.Message!) : JsonError(result.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var r = await _catSvc.DeleteCategoryAsync(id);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCategoryStatus(int id)
    {
        var r = await _catSvc.ToggleCategoryStatusAsync(id, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    // ── SubCategory AJAX ──────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetSubCategory(int id)
    {
        var s = await _catSvc.GetSubCategoryByIdAsync(id);
        return s == null ? NotFound() : Json(s);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSubCategory(CreateSubCategoryDto dto)
    {
        var result = await _catSvc.CreateSubCategoryAsync(dto, CurrentUserId);
        return result.Success ? JsonSuccess(result.Data, result.Message!) : JsonError(result.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSubCategory(int id, CreateSubCategoryDto dto)
    {
        var result = await _catSvc.UpdateSubCategoryAsync(id, dto, CurrentUserId);
        return result.Success ? JsonSuccess(result.Data, result.Message!) : JsonError(result.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSubCategory(int id)
    {
        var r = await _catSvc.DeleteSubCategoryAsync(id);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    // ── Lookup ────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetSubCategoriesByCategory(int categoryId)
        => Json((await _catSvc.GetSubCategoriesAsync(categoryId)).Select(s => new { s.Id, s.Name }));

    private async Task<string> SaveFile(IFormFile file, string folder)
    {
        var dir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(dir);
        var name = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        await using var s = new FileStream(Path.Combine(dir, name), FileMode.Create);
        await file.CopyToAsync(s);
        return $"/uploads/{folder}/{name}";
    }
}