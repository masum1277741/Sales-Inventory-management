namespace ClothingERP.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork uow, IMapper mapper) => (_uow, _mapper) = (uow, mapper);

    // ── Category ─────────────────────────────────────────────────────────
    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        var list = await _uow.Categories.GetQueryable()
            .Include(c => c.SubCategories.Where(s => !s.IsDeleted))
            .Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToListAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(list);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var c = await _uow.Categories.GetByIdAsync(id);
        return c == null ? null : _mapper.Map<CategoryDto>(c);
    }

    public async Task<ServiceResult<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto, int userId)
    {
        if (await _uow.Categories.IsNameExistsAsync(dto.Name))
            return ServiceResult<CategoryDto>.Fail("Category name already exists.");
        var entity = _mapper.Map<Category>(dto);
        entity.CreatedBy = userId;
        await _uow.Categories.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return ServiceResult<CategoryDto>.Ok(_mapper.Map<CategoryDto>(entity), "Category created.");
    }

    public async Task<ServiceResult<CategoryDto>> UpdateCategoryAsync(int id, CreateCategoryDto dto, int userId)
    {
        var entity = await _uow.Categories.GetByIdAsync(id);
        if (entity == null) return ServiceResult<CategoryDto>.Fail("Category not found.");
        if (await _uow.Categories.IsNameExistsAsync(dto.Name, id))
            return ServiceResult<CategoryDto>.Fail("Category name already exists.");
        entity.Name = dto.Name; entity.Description = dto.Description;
        entity.IsActive = dto.IsActive; entity.UpdatedBy = userId;
        if (dto.ImagePath != null) entity.ImagePath = dto.ImagePath;
        _uow.Categories.Update(entity);
        await _uow.SaveChangesAsync();
        return ServiceResult<CategoryDto>.Ok(_mapper.Map<CategoryDto>(entity), "Category updated.");
    }

    public async Task<ServiceResult> DeleteCategoryAsync(int id)
    {
        var entity = await _uow.Categories.GetByIdAsync(id);
        if (entity == null) return ServiceResult.Fail("Not found.");
        if (await _uow.SubCategories.AnyAsync(s => s.CategoryId == id && !s.IsDeleted))
            return ServiceResult.Fail("Cannot delete: has sub-categories.");
        _uow.Categories.Remove(entity);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Deleted.");
    }

    public async Task<ServiceResult> ToggleCategoryStatusAsync(int id, int userId)
    {
        var entity = await _uow.Categories.GetByIdAsync(id);
        if (entity == null) return ServiceResult.Fail("Not found.");
        entity.IsActive = !entity.IsActive; entity.UpdatedBy = userId;
        _uow.Categories.Update(entity); await _uow.SaveChangesAsync();
        return ServiceResult.Ok(entity.IsActive ? "Activated." : "Deactivated.");
    }

    // ── SubCategory ───────────────────────────────────────────────────────
    public async Task<IEnumerable<SubCategoryDto>> GetSubCategoriesAsync(int? categoryId = null)
    {
        var q = _uow.SubCategories.GetQueryable()
            .Include(s => s.Category).Where(s => !s.IsDeleted);
        if (categoryId.HasValue) q = q.Where(s => s.CategoryId == categoryId.Value);
        return _mapper.Map<IEnumerable<SubCategoryDto>>(await q.OrderBy(s => s.Name).ToListAsync());
    }

    public async Task<SubCategoryDto?> GetSubCategoryByIdAsync(int id)
    {
        var s = await _uow.SubCategories.GetQueryable().Include(x => x.Category)
                          .FirstOrDefaultAsync(x => x.Id == id);
        return s == null ? null : _mapper.Map<SubCategoryDto>(s);
    }

    public async Task<ServiceResult<SubCategoryDto>> CreateSubCategoryAsync(CreateSubCategoryDto dto, int userId)
    {
        if (await _uow.SubCategories.IsNameExistsAsync(dto.Name, dto.CategoryId))
            return ServiceResult<SubCategoryDto>.Fail("Sub-category name already exists in this category.");
        var entity = _mapper.Map<SubCategory>(dto);
        entity.CreatedBy = userId;
        await _uow.SubCategories.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return ServiceResult<SubCategoryDto>.Ok(_mapper.Map<SubCategoryDto>(entity), "Sub-category created.");
    }

    public async Task<ServiceResult<SubCategoryDto>> UpdateSubCategoryAsync(int id, CreateSubCategoryDto dto, int userId)
    {
        var entity = await _uow.SubCategories.GetByIdAsync(id);
        if (entity == null) return ServiceResult<SubCategoryDto>.Fail("Not found.");
        if (await _uow.SubCategories.IsNameExistsAsync(dto.Name, dto.CategoryId, id))
            return ServiceResult<SubCategoryDto>.Fail("Name already exists.");
        entity.Name = dto.Name; entity.CategoryId = dto.CategoryId;
        entity.Description = dto.Description; entity.IsActive = dto.IsActive; entity.UpdatedBy = userId;
        _uow.SubCategories.Update(entity); await _uow.SaveChangesAsync();
        return ServiceResult<SubCategoryDto>.Ok(_mapper.Map<SubCategoryDto>(entity), "Updated.");
    }

    public async Task<ServiceResult> DeleteSubCategoryAsync(int id)
    {
        var entity = await _uow.SubCategories.GetByIdAsync(id);
        if (entity == null) return ServiceResult.Fail("Not found.");
        if (await _uow.Products.AnyAsync(p => p.SubCategoryId == id && !p.IsDeleted))
            return ServiceResult.Fail("Cannot delete: has products.");
        _uow.SubCategories.Remove(entity); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Deleted.");
    }
}