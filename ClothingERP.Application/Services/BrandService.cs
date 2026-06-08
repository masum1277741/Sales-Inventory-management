namespace ClothingERP.Application.Services;

public class BrandService : IBrandService
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
    public BrandService(IUnitOfWork uow, IMapper mapper) => (_uow, _mapper) = (uow, mapper);

    public async Task<IEnumerable<BrandDto>> GetAllAsync()
    {
        var list = await _uow.Brands.GetQueryable()
            .Include("Products").Where(b => !b.IsDeleted).OrderBy(b => b.Name).ToListAsync();
        return _mapper.Map<IEnumerable<BrandDto>>(list);
    }

    public async Task<BrandDto?> GetByIdAsync(int id)
    {
        var b = await _uow.Brands.GetByIdAsync(id);
        return b == null ? null : _mapper.Map<BrandDto>(b);
    }

    public async Task<ServiceResult<BrandDto>> CreateAsync(CreateBrandDto dto, int userId)
    {
        if (await _uow.Brands.AnyAsync(b => b.Name.ToLower() == dto.Name.ToLower()))
            return ServiceResult<BrandDto>.Fail("Brand name already exists.");
        var entity = _mapper.Map<Brand>(dto); entity.CreatedBy = userId;
        await _uow.Brands.AddAsync(entity); await _uow.SaveChangesAsync();
        return ServiceResult<BrandDto>.Ok(_mapper.Map<BrandDto>(entity), "Brand created.");
    }

    public async Task<ServiceResult<BrandDto>> UpdateAsync(int id, CreateBrandDto dto, int userId)
    {
        var entity = await _uow.Brands.GetByIdAsync(id);
        if (entity == null) return ServiceResult<BrandDto>.Fail("Not found.");
        if (await _uow.Brands.AnyAsync(b => b.Name.ToLower() == dto.Name.ToLower() && b.Id != id))
            return ServiceResult<BrandDto>.Fail("Name already exists.");
        entity.Name = dto.Name; entity.Description = dto.Description;
        entity.IsActive = dto.IsActive; entity.UpdatedBy = userId;
        if (dto.LogoPath != null) entity.LogoPath = dto.LogoPath;
        _uow.Brands.Update(entity); await _uow.SaveChangesAsync();
        return ServiceResult<BrandDto>.Ok(_mapper.Map<BrandDto>(entity), "Updated.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var entity = await _uow.Brands.GetByIdAsync(id);
        if (entity == null) return ServiceResult.Fail("Not found.");
        if (await _uow.Products.AnyAsync(p => p.BrandId == id && !p.IsDeleted))
            return ServiceResult.Fail("Cannot delete: has products.");
        _uow.Brands.Remove(entity); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Deleted.");
    }

    public async Task<ServiceResult> ToggleStatusAsync(int id, int userId)
    {
        var entity = await _uow.Brands.GetByIdAsync(id);
        if (entity == null) return ServiceResult.Fail("Not found.");
        entity.IsActive = !entity.IsActive; entity.UpdatedBy = userId;
        _uow.Brands.Update(entity); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Status toggled.");
    }
}