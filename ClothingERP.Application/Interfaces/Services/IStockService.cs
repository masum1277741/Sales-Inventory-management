namespace ClothingERP.Application.Interfaces.Services;

public interface IStockService
{
    Task<IEnumerable<StockListDto>> GetAllAsync(int? branchId = null);
    Task<StockDto?> GetByVariantIdAsync(int variantId, int? branchId = null);
    Task<IEnumerable<StockListDto>> GetLowStockAsync(int? branchId = null);
    Task<IEnumerable<StockListDto>> GetOutOfStockAsync(int? branchId = null);
    Task<decimal> GetTotalStockValueAsync(int? branchId = null);
    Task<decimal> GetTotalRetailStockValueAsync(int? branchId = null);
    Task<ServiceResult> AdjustStockAsync(StockAdjustmentDto dto, int userId);
    Task UpdateStockAsync(int variantId, int branchId, decimal quantity, StockMovementType type,
                          string referenceNumber, int userId);
    Task<decimal> GetVariantQuantityAsync(int variantId, int? branchId = null);
}
