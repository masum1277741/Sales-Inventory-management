namespace ClothingERP.Infrastructure.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Product?> GetWithVariantsAsync(int productId)
        => await _dbSet
            .Include(p => p.Category)
            .Include(p => p.SubCategory)
            .Include(p => p.Brand)
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
                .ThenInclude(v => v.Size)
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
                .ThenInclude(v => v.Color)
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
                .ThenInclude(v => v.Stock)
            .FirstOrDefaultAsync(p => p.Id == productId);

    public async Task<IEnumerable<Product>> GetWithDetailsAsync()
        => await _dbSet
            .Include(p => p.Category)
            .Include(p => p.SubCategory)
            .Include(p => p.Brand)
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
            .OrderBy(p => p.Name)
            .ToListAsync();

    public async Task<bool> IsSkuExistsAsync(string sku, int? excludeId = null)
    {
        var q = _dbSet.Where(p => p.SKU.ToLower() == sku.ToLower());
        if (excludeId.HasValue) q = q.Where(p => p.Id != excludeId.Value);
        return await q.AnyAsync();
    }
    public async Task<IEnumerable<Product>> GetAllWithDetailsAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SubCategory)
            .Include(p => p.Brand)
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
                .ThenInclude(v => v.Size)
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
                .ThenInclude(v => v.Color)
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
                .ThenInclude(v => v.Stock)
            .Where(p => !p.IsDeleted)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Product?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SubCategory)
            .Include(p => p.Brand)
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
                .ThenInclude(v => v.Size)
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
                .ThenInclude(v => v.Color)
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
                .ThenInclude(v => v.Stock)
            .Where(p => !p.IsDeleted && p.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
    public async Task<IEnumerable<Product>> SearchAsync(string keyword)
        => await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
                .ThenInclude(v => v.Size)
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
                .ThenInclude(v => v.Color)
            .Where(p => p.IsActive &&
                       (p.Name.Contains(keyword) || p.SKU.Contains(keyword)))
            .Take(50)
            .ToListAsync();

    public async Task<int> GetNextSkuSequenceAsync()
        => await _dbSet.CountAsync() + 1;
}