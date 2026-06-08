namespace ClothingERP.Application.Interfaces.Repositories;

public interface IGoodsReceiptNoteRepository : IRepository<GoodsReceiptNote>
{
    Task<GoodsReceiptNote?> GetWithDetailsAsync(int id);
    Task<IEnumerable<GoodsReceiptNote>> GetByPurchaseOrderAsync(int purchaseOrderId);
    Task<string> GenerateGRNNumberAsync();
}