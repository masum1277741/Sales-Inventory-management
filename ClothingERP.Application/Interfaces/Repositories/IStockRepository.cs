namespace ClothingERP.Application.Interfaces.Repositories;

public interface IStockRepository : IRepository<Stock>
{
    Task<Stock?> GetByVariantIdAsync(int variantId);
    Task<Stock?> GetWithMovementsAsync(int stockId);
    Task<IEnumerable<Stock>> GetWithDetailsAsync();
    Task<IEnumerable<Stock>> GetLowStockAsync(int? branchId = null);
    Task<Stock?> GetByVariantAndBranchAsync(int variantId, int branchId);
    Task<IEnumerable<Stock>> GetOutOfStockAsync(int? branchId = null);
    Task<bool> TryDecrementAsync(int variantId, int branchId, int quantity);
    Task<bool> IncrementAsync(int variantId, int branchId, int quantity);
    Task<decimal> GetTotalStockValueAsync(int? branchId = null);
    Task<decimal> GetTotalRetailStockValueAsync(int? branchId = null);
    Task<List<Stock>> GetAllForBranchAsync(int branchId);
    Task<List<Stock>> GetAllVariantStockAcrossBranchesAsync(int variantId);
    //Task IncrementAsync(int productVariantId, int branchId, decimal quantity);
}
