namespace ClothingERP.Application.Interfaces.Repositories;

public interface IStockRepository : IRepository<Stock>
{
    Task<Stock?> GetByVariantIdAsync(int variantId);
    Task<Stock?> GetWithMovementsAsync(int stockId);
    Task<IEnumerable<Stock>> GetWithDetailsAsync();
    Task<IEnumerable<Stock>> GetLowStockAsync();
    Task<IEnumerable<Stock>> GetOutOfStockAsync();
    Task<decimal> GetTotalStockValueAsync();
}