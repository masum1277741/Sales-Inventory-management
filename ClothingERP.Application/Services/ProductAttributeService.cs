namespace ClothingERP.Application.Services;

public class ProductAttributeService : IProductAttributeService
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
    public ProductAttributeService(IUnitOfWork uow, IMapper mapper) => (_uow, _mapper) = (uow, mapper);

    public async Task<IEnumerable<SizeDto>> GetSizesAsync()
        => _mapper.Map<IEnumerable<SizeDto>>(await _uow.Sizes.GetQueryable()
           .Where(s => !s.IsDeleted).OrderBy(s => s.SortOrder).ToListAsync());

    public async Task<ServiceResult<SizeDto>> CreateSizeAsync(CreateSizeDto dto, int userId)
    {
        if (await _uow.Sizes.AnyAsync(s => s.Name.ToLower() == dto.Name.ToLower()))
            return ServiceResult<SizeDto>.Fail("Size already exists.");
        var entity = _mapper.Map<Size>(dto); entity.CreatedBy = userId;
        await _uow.Sizes.AddAsync(entity); await _uow.SaveChangesAsync();
        return ServiceResult<SizeDto>.Ok(_mapper.Map<SizeDto>(entity), "Size created.");
    }

    public async Task<ServiceResult<SizeDto>> UpdateSizeAsync(int id, CreateSizeDto dto, int userId)
    {
        var entity = await _uow.Sizes.GetByIdAsync(id);
        if (entity == null) return ServiceResult<SizeDto>.Fail("Not found.");
        entity.Name = dto.Name; entity.SortOrder = dto.SortOrder; entity.UpdatedBy = userId;
        _uow.Sizes.Update(entity); await _uow.SaveChangesAsync();
        return ServiceResult<SizeDto>.Ok(_mapper.Map<SizeDto>(entity), "Updated.");
    }

    public async Task<ServiceResult> DeleteSizeAsync(int id)
    {
        var entity = await _uow.Sizes.GetByIdAsync(id);
        if (entity == null) return ServiceResult.Fail("Not found.");
        _uow.Sizes.Remove(entity); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Deleted.");
    }

    public async Task<IEnumerable<ColorDto>> GetColorsAsync()
        => _mapper.Map<IEnumerable<ColorDto>>(await _uow.Colors.GetQueryable()
           .Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToListAsync());

    public async Task<ServiceResult<ColorDto>> CreateColorAsync(CreateColorDto dto, int userId)
    {
        if (await _uow.Colors.AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower()))
            return ServiceResult<ColorDto>.Fail("Color already exists.");
        var entity = _mapper.Map<Color>(dto); entity.CreatedBy = userId;
        await _uow.Colors.AddAsync(entity); await _uow.SaveChangesAsync();
        return ServiceResult<ColorDto>.Ok(_mapper.Map<ColorDto>(entity), "Color created.");
    }

    public async Task<ServiceResult<ColorDto>> UpdateColorAsync(int id, CreateColorDto dto, int userId)
    {
        var entity = await _uow.Colors.GetByIdAsync(id);
        if (entity == null) return ServiceResult<ColorDto>.Fail("Not found.");
        entity.Name = dto.Name; entity.HexCode = dto.HexCode; entity.UpdatedBy = userId;
        _uow.Colors.Update(entity); await _uow.SaveChangesAsync();
        return ServiceResult<ColorDto>.Ok(_mapper.Map<ColorDto>(entity), "Updated.");
    }

    public async Task<ServiceResult> DeleteColorAsync(int id)
    {
        var entity = await _uow.Colors.GetByIdAsync(id);
        if (entity == null) return ServiceResult.Fail("Not found.");
        _uow.Colors.Remove(entity); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Deleted.");
    }
}