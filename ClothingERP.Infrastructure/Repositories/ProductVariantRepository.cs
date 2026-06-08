namespace ClothingERP.Infrastructure.Repositories;

public class ProductVariantRepository : GenericRepository<ProductVariant>, IProductVariantRepository
{
    public ProductVariantRepository(ApplicationDbContext context) : base(context) { }

    public async Task<ProductVariant?> GetByBarcodeAsync(string barcode)
        => await _dbSet
            .Include(v => v.Product).ThenInclude(p => p.Category)
            .Include(v => v.Size)
            .Include(v => v.Color)
            .Include(v => v.Stock)
            .FirstOrDefaultAsync(v => v.Barcode == barcode && v.IsActive);

    public async Task<ProductVariant?> GetWithFullDetailsAsync(int variantId)
        => await _dbSet
            .Include(v => v.Product).ThenInclude(p => p.Brand)
            .Include(v => v.Size)
            .Include(v => v.Color)
            .Include(v => v.Stock)
            .FirstOrDefaultAsync(v => v.Id == variantId);

    public async Task<IEnumerable<ProductVariant>> GetByProductIdAsync(int productId)
        => await _dbSet
            .Include(v => v.Size)
            .Include(v => v.Color)
            .Include(v => v.Stock)
            .Where(v => v.ProductId == productId)
            .ToListAsync();

    public async Task<bool> IsBarcodeExistsAsync(string barcode, int? excludeId = null)
    {
        var q = _dbSet.Where(v => v.Barcode == barcode);
        if (excludeId.HasValue) q = q.Where(v => v.Id != excludeId.Value);
        return await q.AnyAsync();
    }

    public async Task<bool> SizeColorCombinationExistsAsync(int productId, int sizeId, int colorId, int? excludeId = null)
    {
        var q = _dbSet.Where(v => v.ProductId == productId && v.SizeId == sizeId && v.ColorId == colorId);
        if (excludeId.HasValue) q = q.Where(v => v.Id != excludeId.Value);
        return await q.AnyAsync();
    }
}