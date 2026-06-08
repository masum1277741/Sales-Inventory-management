namespace ClothingERP.Infrastructure.Repositories;

public class AccountTransactionRepository : GenericRepository<AccountTransaction>, IAccountTransactionRepository
{
    public AccountTransactionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<AccountTransaction>> GetByDateRangeAsync(DateTime from, DateTime to)
        => await _dbSet.Where(t => t.TransactionDate >= from && t.TransactionDate <= to.AddDays(1))
                       .OrderByDescending(t => t.TransactionDate).ToListAsync();

    public async Task<IEnumerable<AccountTransaction>> GetByTypeAsync(TransactionType type, DateTime? from = null, DateTime? to = null)
    {
        var q = _dbSet.Where(t => t.TransactionType == type);
        if (from.HasValue) q = q.Where(t => t.TransactionDate >= from.Value);
        if (to.HasValue) q = q.Where(t => t.TransactionDate <= to.Value.AddDays(1));
        return await q.OrderByDescending(t => t.TransactionDate).ToListAsync();
    }

    public async Task<decimal> GetTotalIncomeAsync(DateTime from, DateTime to)
        => await _dbSet.Where(t => t.TransactionType == TransactionType.Income &&
                                    t.TransactionDate >= from && t.TransactionDate <= to.AddDays(1))
                       .SumAsync(t => t.Amount);

    public async Task<decimal> GetTotalExpenseAsync(DateTime from, DateTime to)
        => await _dbSet.Where(t => t.TransactionType == TransactionType.Expense &&
                                    t.TransactionDate >= from && t.TransactionDate <= to.AddDays(1))
                       .SumAsync(t => t.Amount);

    public async Task<decimal> GetCashBalanceAsync()
    {
        var income = await _dbSet.Where(t => t.TransactionType == TransactionType.Income).SumAsync(t => t.Amount);
        var expense = await _dbSet.Where(t => t.TransactionType == TransactionType.Expense).SumAsync(t => t.Amount);
        return income - expense;
    }

    public async Task<string> GenerateTransactionNumberAsync()
    {
        var today = DateTime.Now;
        var count = await _dbSet.CountAsync(t => t.TransactionDate.Date == today.Date) + 1;
        return $"TXN-{today:yyyyMMdd}-{count:D4}";
    }
}