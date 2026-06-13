namespace ClothingERP.Application.Interfaces.Repositories;

public interface IProductVariantRepository : IRepository<ProductVariant>
{
    Task<ProductVariant?> GetByBarcodeAsync(string barcode);
    Task<ProductVariant?> GetWithFullDetailsAsync(int variantId);
    Task<IEnumerable<ProductVariant>> GetByProductIdAsync(int productId);
    Task<IEnumerable<ProductVariant>> GetAllWithDetailsAsync();
    Task<bool> IsBarcodeExistsAsync(string barcode, int? excludeId = null);
    Task<bool> SizeColorCombinationExistsAsync(int productId, int sizeId, int colorId, int? excludeId = null);
}