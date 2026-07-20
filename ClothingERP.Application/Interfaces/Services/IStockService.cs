namespace ClothingERP.Application.Interfaces.Services;

public interface IStockService
{
    Task<IEnumerable<StockListDto>> GetAllAsync();
    Task<StockDto?> GetByVariantIdAsync(int variantId);
    Task<IEnumerable<StockListDto>> GetLowStockAsync(int? branchId = null);
    Task<IEnumerable<StockListDto>> GetOutOfStockAsync(int? branchId = null);
    Task<decimal> GetTotalStockValueAsync(int? branchId = null);
    Task<ServiceResult> AdjustStockAsync(StockAdjustmentDto dto, int userId);
    Task UpdateStockAsync(int variantId, decimal quantity, StockMovementType type,
                          string referenceNumber, int userId);
}