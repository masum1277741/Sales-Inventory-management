namespace ClothingERP.Application.Interfaces.Services;

public interface IStorefrontService
{
    Task<PagedResultDto<StorefrontProductDto>> GetProductsAsync(ProductFilterDto filter);
    Task<StorefrontProductDetailDto?> GetProductDetailAsync(int productId);
    Task<List<StorefrontProductDto>> GetFeaturedProductsAsync(int count = 8);
    Task<StorefrontSettingsDto> GetSettingsAsync();
    Task<ServiceResult> UpdateSettingsAsync(StorefrontSettingsDto dto, int userId);
}