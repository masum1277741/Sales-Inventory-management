namespace ClothingERP.Application.Interfaces.Repositories;

public interface IPurchaseOrderRepository : IRepository<PurchaseOrder>
{
    Task<PurchaseOrder?> GetWithDetailsAsync(int id);
    Task<IEnumerable<PurchaseOrder>> GetBySupplierAsync(int supplierId);
    Task<IEnumerable<PurchaseOrder>> GetByStatusAsync(PurchaseOrderStatus status);
    Task<IEnumerable<PurchaseOrder>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<string> GeneratePONumberAsync();
}