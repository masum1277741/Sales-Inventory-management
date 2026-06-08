namespace ClothingERP.Application.Interfaces.Repositories;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<Supplier?> GetWithDetailsAsync(int supplierId);
    Task<IEnumerable<Supplier>> GetWithDueBalanceAsync();
    Task<IEnumerable<Supplier>> SearchAsync(string keyword);
    Task<bool> IsPhoneExistsAsync(string phone, int? excludeId = null);
}