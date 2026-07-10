namespace ClothingERP.Application.Interfaces.Services;

public interface IBundleService
{
    Task<IEnumerable<ProductBundleListDto>> GetAllAsync();
    Task<ProductBundleDto?> GetByIdAsync(int id);
    Task<ServiceResult<ProductBundleDto>> CreateAsync(CreateProductBundleDto dto, int userId);
    Task<ServiceResult<ProductBundleDto>> UpdateAsync(int id, CreateProductBundleDto dto, int userId);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> ToggleStatusAsync(int id, int userId);

    Task<IEnumerable<BundleSearchDto>> SearchBundlesAsync(string keyword);
    Task<int> GetAvailableStockAsync(int bundleId);
}