namespace ClothingERP.Application.Interfaces.Services;

public interface IProductService
{
    Task<IEnumerable<ProductListDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ServiceResult<ProductDto>> CreateAsync(CreateProductDto dto, int userId);
    Task<ServiceResult<ProductDto>> UpdateAsync(int id, UpdateProductDto dto, int userId);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> ToggleStatusAsync(int id, int userId);

    // Variant operations
    Task<IEnumerable<ProductVariantDto>> GetAllActiveVariantsAsync();
    Task<ProductVariantDto?> GetVariantByBarcodeAsync(string barcode);
    Task<IEnumerable<ProductVariantDto>> SearchVariantsAsync(string keyword);
    Task<ServiceResult<ProductVariantDto>> AddVariantAsync(int productId, CreateProductVariantDto dto, int userId);
    Task<ServiceResult> DeleteVariantAsync(int variantId);
    Task<BulkActionResultDto> BulkUpdatePriceAsync(BulkPriceUpdateDto dto, int userId);
    Task<BulkActionResultDto> BulkToggleStatusAsync(BulkStatusUpdateDto dto, int userId);
    Task<BulkActionResultDto> BulkDeleteAsync(BulkDeleteDto dto);
    Task<BulkActionResultDto> BulkUpdateCategoryAsync(BulkCategoryUpdateDto dto, int userId);
    // Barcode
    Task<ServiceResult> RegenerateBarcodeAsync(int variantId, int userId);
}