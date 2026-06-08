namespace ClothingERP.Application.Services;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
    public AccountService(IUnitOfWork uow, IMapper mapper) => (_uow, _mapper) = (uow, mapper);

    public async Task<IEnumerable<AccountTransactionListDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<AccountTransactionListDto>>(await _uow.AccountTransactions.GetQueryable()
           .Where(t => !t.IsDeleted).OrderByDescending(t => t.TransactionDate).ToListAsync());

    public async Task<AccountTransactionDto?> GetByIdAsync(int id)
    {
        var t = await _uow.AccountTransactions.GetByIdAsync(id);
        return t == null ? null : _mapper.Map<AccountTransactionDto>(t);
    }

    public async Task<ServiceResult<AccountTransactionDto>> CreateAsync(CreateAccountTransactionDto dto, int userId)
    {
        var entity = _mapper.Map<AccountTransaction>(dto);
        entity.TransactionNumber = await _uow.AccountTransactions.GenerateTransactionNumberAsync();
        entity.CreatedBy = userId;
        await _uow.AccountTransactions.AddAsync(entity); await _uow.SaveChangesAsync();
        return ServiceResult<AccountTransactionDto>.Ok(_mapper.Map<AccountTransactionDto>(entity), "Transaction recorded.");
    }

    public async Task<ServiceResult> UpdateAsync(int id, CreateAccountTransactionDto dto, int userId)
    {
        var entity = await _uow.AccountTransactions.GetByIdAsync(id);
        if (entity == null) return ServiceResult.Fail("Not found.");
        entity.TransactionType = dto.TransactionType; entity.Category = dto.Category;
        entity.Amount = dto.Amount; entity.Description = dto.Description;
        entity.TransactionDate = dto.TransactionDate; entity.PaymentMethod = dto.PaymentMethod;
        entity.ReferenceNumber = dto.ReferenceNumber; entity.Notes = dto.Notes; entity.UpdatedBy = userId;
        _uow.AccountTransactions.Update(entity); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Updated.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var entity = await _uow.AccountTransactions.GetByIdAsync(id);
        if (entity == null) return ServiceResult.Fail("Not found.");
        _uow.AccountTransactions.Remove(entity); await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Deleted.");
    }

    public async Task<decimal> GetCashBalanceAsync()
        => await _uow.AccountTransactions.GetCashBalanceAsync();

    public async Task<ProfitLossDto> GetProfitLossAsync(DateTime from, DateTime to)
    {
        var today = DateTime.UtcNow.Date;
        var invoices = await _uow.SalesInvoices.GetByDateRangeAsync(from, to);
        var validInv = invoices.Where(i => i.Status != InvoiceStatus.Cancelled).ToList();

        var totalSales = validInv.Sum(i => i.TotalAmount);
        var totalDiscount = validInv.Sum(i => i.DiscountAmount);
        var netSales = totalSales - totalDiscount;

        var expenses = await _uow.AccountTransactions.GetTotalExpenseAsync(from, to);
        var expByCategory = (await _uow.AccountTransactions.GetByTypeAsync(TransactionType.Expense, from, to))
            .GroupBy(t => t.Category.ToString())
            .Select(g => new ExpenseSummaryDto { Category = g.Key, Amount = g.Sum(t => t.Amount) }).ToList();

        // COGS estimate (will be more accurate with invoice items)
        return new ProfitLossDto
        {
            FromDate = from,
            ToDate = to,
            TotalSales = totalSales,
            TotalDiscount = totalDiscount,
            NetSales = netSales,
            CostOfGoodsSold = 0, // calculated in detailed report
            GrossProfit = netSales,
            TotalExpenses = expenses,
            NetProfit = netSales - expenses,
            ExpenseBreakdown = expByCategory
        };
    }
}