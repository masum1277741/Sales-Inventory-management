namespace ClothingERP.Application.Interfaces.Services;

public interface IStockTransferService
{
    Task<IEnumerable<StockTransferListDto>> GetAllAsync(int? branchId = null);
    Task<StockTransferDto?> GetByIdAsync(int id);
    Task<ServiceResult<StockTransferDto>> CreateAsync(CreateStockTransferDto dto, int fromBranchId, int userId);
    Task<ServiceResult> ReceiveAsync(ReceiveStockTransferDto dto, int userId);
    Task<ServiceResult> CancelAsync(int id, int userId);
}