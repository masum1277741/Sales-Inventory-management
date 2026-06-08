namespace ClothingERP.Application.Interfaces.Repositories;

public interface IPurchaseReturnRepository : IRepository<PurchaseReturn>
{
    Task<PurchaseReturn?> GetWithDetailsAsync(int id);
    Task<IEnumerable<PurchaseReturn>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<string> GenerateReturnNumberAsync();
}