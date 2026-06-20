namespace ClothingERP.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<IEnumerable<CustomerListDto>> GetAllAsync();
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto?> GetByPhoneAsync(string phone);
    Task<ServiceResult<CustomerDto>> CreateAsync(CreateCustomerDto dto, int userId);
    Task<ServiceResult<CustomerDto>> UpdateAsync(int id, UpdateCustomerDto dto, int userId);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> ToggleStatusAsync(int id, int userId);

    // Ledger & Payments
    Task<IEnumerable<CustomerLedgerDto>> GetLedgerAsync(int customerId, DateTime? from = null, DateTime? to = null);
    Task<decimal> GetBalanceAsync(int customerId);
    Task<ServiceResult> AddPaymentAsync(int customerId, decimal amount, string description,
                                        string? reference, int userId);
    Task<IEnumerable<CustomerLedgerDto>> GetLedgerAsync(int customerId, DateTime from, DateTime to);

    // Groups
    Task<IEnumerable<CustomerGroupDto>> GetGroupsAsync();
    Task<ServiceResult<CustomerGroupDto>> CreateGroupAsync(CreateCustomerGroupDto dto, int userId);
}