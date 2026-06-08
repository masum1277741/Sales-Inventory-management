namespace ClothingERP.Application.Interfaces.Services;

public interface IPurchaseService
{
    // Purchase Orders
    Task<IEnumerable<PurchaseOrderListDto>> GetAllOrdersAsync();
    Task<PurchaseOrderDto?> GetOrderByIdAsync(int id);
    Task<ServiceResult<PurchaseOrderDto>> CreateOrderAsync(CreatePurchaseOrderDto dto, int userId);
    Task<ServiceResult<PurchaseOrderDto>> UpdateOrderAsync(int id, CreatePurchaseOrderDto dto, int userId);
    Task<ServiceResult> ApproveOrderAsync(int id, int userId);
    Task<ServiceResult> CancelOrderAsync(int id, string reason, int userId);

    // Goods Receipt Notes
    Task<IEnumerable<GRNListDto>> GetAllGRNsAsync();
    Task<GRNDto?> GetGRNByIdAsync(int id);
    Task<ServiceResult<GRNDto>> CreateGRNAsync(CreateGRNDto dto, int userId);

    // Supplier payment against PO
    Task<ServiceResult> AddSupplierPaymentAsync(int purchaseOrderId, decimal amount,
                                                 PaymentMethod method, string? reference, int userId);
}