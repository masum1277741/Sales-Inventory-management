namespace ClothingERP.Application.Interfaces.Repositories;

public interface IAccountTransactionRepository : IRepository<AccountTransaction>
{
    Task<IEnumerable<AccountTransaction>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<AccountTransaction>> GetByTypeAsync(TransactionType type, DateTime? from = null, DateTime? to = null);
    Task<decimal> GetTotalIncomeAsync(DateTime from, DateTime to);
    Task<decimal> GetTotalExpenseAsync(DateTime from, DateTime to);
    Task<decimal> GetCashBalanceAsync();
    Task<string> GenerateTransactionNumberAsync();
}