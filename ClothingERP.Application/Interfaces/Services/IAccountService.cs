namespace ClothingERP.Application.Interfaces.Services;

public interface IAccountService
{
    Task<IEnumerable<AccountTransactionListDto>> GetAllAsync();
    Task<AccountTransactionDto?> GetByIdAsync(int id);
    Task<ServiceResult<AccountTransactionDto>> CreateAsync(CreateAccountTransactionDto dto, int userId);
    Task<ServiceResult> UpdateAsync(int id, CreateAccountTransactionDto dto, int userId);
    Task<ServiceResult> DeleteAsync(int id);
    Task<decimal> GetCashBalanceAsync();
    Task<ProfitLossDto> GetProfitLossAsync(DateTime from, DateTime to);
}