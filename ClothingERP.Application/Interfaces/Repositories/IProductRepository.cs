namespace ClothingERP.Application.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetWithVariantsAsync(int productId);
    Task<IEnumerable<Product>> GetWithDetailsAsync();
    Task<bool> IsSkuExistsAsync(string sku, int? excludeId = null);
    Task<IEnumerable<Product>> SearchAsync(string keyword);
    Task<int> GetNextSkuSequenceAsync();
}