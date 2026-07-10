using ClothingERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClothingERP.Infrastructure.Repositories;

public class StockRepository : GenericRepository<Stock>, IStockRepository
{
    public StockRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Stock?> GetByVariantIdAsync(int variantId)
        => await _dbSet
            .Include(s => s.ProductVariant).ThenInclude(v => v.Product)
            .Include(s => s.ProductVariant).ThenInclude(v => v.Size)
            .Include(s => s.ProductVariant).ThenInclude(v => v.Color)
            .FirstOrDefaultAsync(s => s.ProductVariantId == variantId && !s.IsDeleted);

    public async Task<Stock?> GetWithMovementsAsync(int stockId)
        => await _dbSet
            .Include(s => s.ProductVariant).ThenInclude(v => v.Product)
            .Include(s => s.ProductVariant).ThenInclude(v => v.Size)
            .Include(s => s.ProductVariant).ThenInclude(v => v.Color)
            .Include(s => s.StockMovements.Where(m => !m.IsDeleted).OrderByDescending(m => m.MovementDate).Take(50))
            .FirstOrDefaultAsync(s => s.Id == stockId);

    public async Task<IEnumerable<Stock>> GetWithDetailsAsync()
        => await _dbSet
            .Include(s => s.ProductVariant).ThenInclude(v => v.Product).ThenInclude(p => p.Category)
            .Include(s => s.ProductVariant).ThenInclude(v => v.Size)
            .Include(s => s.ProductVariant).ThenInclude(v => v.Color)
            .OrderBy(s => s.ProductVariant.Product.Name)
            .ToListAsync();

    public async Task<IEnumerable<Stock>> GetLowStockAsync()
        => await _dbSet
            .Include(s => s.ProductVariant).ThenInclude(v => v.Product).ThenInclude(p => p.Category)
            .Include(s => s.ProductVariant).ThenInclude(v => v.Size)
            .Include(s => s.ProductVariant).ThenInclude(v => v.Color)
            .Where(s => s.Quantity > 0 && s.Quantity <= s.ProductVariant.Product.ReorderPoint)
            .ToListAsync();

    public async Task<IEnumerable<Stock>> GetOutOfStockAsync()
        => await _dbSet
            .Include(s => s.ProductVariant).ThenInclude(v => v.Product)
            .Include(s => s.ProductVariant).ThenInclude(v => v.Size)
            .Include(s => s.ProductVariant).ThenInclude(v => v.Color)
            .Where(s => s.Quantity <= 0)
            .ToListAsync();

    public async Task<decimal> GetTotalStockValueAsync()
        => await _dbSet
            .Where(s => s.Quantity > 0)
            .SumAsync(s => s.Quantity * (s.ProductVariant.CostPriceOverride ?? s.ProductVariant.Product.CostPrice));

    public async Task<bool> TryDecrementAsync(int variantId, int quantity)
    {
        var rows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE Stocks
            SET Quantity = Quantity - {quantity}, UpdatedAt = {DateTime.UtcNow}
            WHERE ProductVariantId = {variantId}
              AND Quantity >= {quantity}
              AND IsDeleted = 0");

        return rows > 0;  
    }

    public async Task<bool> IncrementAsync(int variantId, int quantity)
    {
        var rows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE Stocks
            SET Quantity = Quantity + {quantity}, UpdatedAt = {DateTime.UtcNow}
            WHERE ProductVariantId = {variantId}
              AND IsDeleted = 0");

        return rows > 0;
    }
}