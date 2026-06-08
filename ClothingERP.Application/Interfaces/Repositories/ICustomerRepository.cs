namespace ClothingERP.Application.Interfaces.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetWithDetailsAsync(int customerId);
    Task<IEnumerable<Customer>> GetWithDueBalanceAsync();
    Task<IEnumerable<Customer>> SearchAsync(string keyword);
    Task<Customer?> GetByPhoneAsync(string phone);
    Task<bool> IsPhoneExistsAsync(string phone, int? excludeId = null);
}