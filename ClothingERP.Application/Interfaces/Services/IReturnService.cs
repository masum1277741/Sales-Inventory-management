namespace ClothingERP.Application.Interfaces.Services;

public interface IReturnService
{
    // Sales Returns
    Task<IEnumerable<SalesReturnListDto>> GetAllSalesReturnsAsync();
    Task<SalesReturnDto?> GetSalesReturnByIdAsync(int id);
    Task<ServiceResult<SalesReturnDto>> CreateSalesReturnAsync(CreateSalesReturnDto dto, int userId);

    // Purchase Returns
    Task<IEnumerable<PurchaseReturnListDto>> GetAllPurchaseReturnsAsync();
    Task<PurchaseReturnDto?> GetPurchaseReturnByIdAsync(int id);
    Task<ServiceResult<PurchaseReturnDto>> CreatePurchaseReturnAsync(CreatePurchaseReturnDto dto, int userId);
}