namespace ClothingERP.Web.Controllers;

public class BrandController : BaseController
{
    private readonly IBrandService _brandSvc;
    private readonly IWebHostEnvironment _env;

    public BrandController(IBrandService brandSvc, IWebHostEnvironment env)
        => (_brandSvc, _env) = (brandSvc, env);

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Brand Management";
        return View(await _brandSvc.GetAllAsync());
    }

    [HttpGet]
    public async Task<IActionResult> GetBrand(int id)
    {
        var b = await _brandSvc.GetByIdAsync(id);
        return b == null ? NotFound() : Json(b);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBrandDto dto, IFormFile? logo)
    {
        if (logo is { Length: > 0 }) dto.LogoPath = await SaveFile(logo, "brands");
        var r = await _brandSvc.CreateAsync(dto, CurrentUserId);
        return r.Success ? JsonSuccess(r.Data, r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateBrandDto dto, IFormFile? logo)
    {
        if (logo is { Length: > 0 }) dto.LogoPath = await SaveFile(logo, "brands");
        var r = await _brandSvc.UpdateAsync(id, dto, CurrentUserId);
        return r.Success ? JsonSuccess(r.Data, r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var r = await _brandSvc.DeleteAsync(id);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var r = await _brandSvc.ToggleStatusAsync(id, CurrentUserId);
        return r.Success ? JsonSuccess(message: r.Message!) : JsonError(r.Message!);
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
}