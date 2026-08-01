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

    // ── Branch-specific Lookup ────────────────────────────────────────────
    public async Task<Stock?> GetByVariantAndBranchAsync(int variantId, int branchId)
        => await _dbSet
            .Include(s => s.ProductVariant).ThenInclude(v => v.Product)
            .Include(s => s.ProductVariant).ThenInclude(v => v.Size)
            .Include(s => s.ProductVariant).ThenInclude(v => v.Color)
            .FirstOrDefaultAsync(s => s.ProductVariantId == variantId && s.BranchId == branchId && !s.IsDeleted);

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

    public async Task<IEnumerable<Stock>> GetLowStockAsync(int? branchId = null)
    {
        var query = _dbSet
            .Include(s => s.ProductVariant).ThenInclude(v => v.Product).ThenInclude(p => p.Category)
            .Include(s => s.ProductVariant).ThenInclude(v => v.Size)
            .Include(s => s.ProductVariant).ThenInclude(v => v.Color)
            .Where(s => s.Quantity > 0 && s.Quantity <= s.ProductVariant.Product.ReorderPoint);

        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Stock>> GetOutOfStockAsync(int? branchId = null)
    {
        var query = _dbSet
            .Include(s => s.ProductVariant).ThenInclude(v => v.Product)
            .Include(s => s.ProductVariant).ThenInclude(v => v.Size)
            .Include(s => s.ProductVariant).ThenInclude(v => v.Color)
            .Where(s => s.Quantity <= 0);

        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);

        return await query.ToListAsync();
    }

    public async Task<decimal> GetTotalStockValueAsync(int? branchId = null)
    {
        var query = _dbSet.Where(s => s.Quantity > 0);

        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);

        return await query
            .SumAsync(s => s.Quantity * (s.ProductVariant.RetailPriceOverride ?? s.ProductVariant.Product.RetailPrice));
    }

    public async Task<decimal> GetTotalRetailStockValueAsync(int? branchId = null)
    {
        return await GetTotalStockValueAsync(branchId);
    }

    // ── Branch-aware Stock Listing ───────────────────────────────────────
    public async Task<List<Stock>> GetAllForBranchAsync(int branchId)
        => await _dbSet
            .Include(s => s.ProductVariant).ThenInclude(v => v.Product)
            .Where(s => s.BranchId == branchId && !s.IsDeleted)
            .ToListAsync();

    public async Task<List<Stock>> GetAllVariantStockAcrossBranchesAsync(int variantId)
        => await _dbSet
            .Include(s => s.Branch)
            .Where(s => s.ProductVariantId == variantId && !s.IsDeleted)
            .ToListAsync();


    public async Task<bool> TryDecrementAsync(int variantId, int branchId, int quantity)
    {
        var rows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE Stocks
            SET Quantity = Quantity - {quantity}, UpdatedAt = {DateTime.UtcNow}
            WHERE ProductVariantId = {variantId}
              AND BranchId = {branchId}
              AND Quantity >= {quantity}
              AND IsDeleted = 0");
        return rows > 0;
    }

    public async Task<bool> IncrementAsync(int variantId, int branchId, int quantity)
    {
        
        var existing = await GetByVariantAndBranchAsync(variantId, branchId);
        if (existing == null)
        {
            await _context.Stocks.AddAsync(new Stock
            {
                ProductVariantId = variantId,
                BranchId = branchId,
                Quantity = quantity
            });
            await _context.SaveChangesAsync();
            return true;
        }

        var rows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE Stocks
            SET Quantity = Quantity + {quantity}, UpdatedAt = {DateTime.UtcNow}
            WHERE ProductVariantId = {variantId}
              AND BranchId = {branchId}
              AND IsDeleted = 0");
        return rows > 0;
    }
}