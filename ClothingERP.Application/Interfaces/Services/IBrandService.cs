namespace ClothingERP.Application.Interfaces.Services;

public interface IBrandService
{
    Task<IEnumerable<BrandDto>> GetAllAsync();
    Task<BrandDto?> GetByIdAsync(int id);
    Task<ServiceResult<BrandDto>> CreateAsync(CreateBrandDto dto, int userId);
    Task<ServiceResult<BrandDto>> UpdateAsync(int id, CreateBrandDto dto, int userId);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> ToggleStatusAsync(int id, int userId);
}