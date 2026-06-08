namespace ClothingERP.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow; private readonly IMapper _mapper;
    public ReportService(IUnitOfWork uow, IMapper mapper) => (_uow, _mapper) = (uow, mapper);

    public async Task<IEnumerable<SalesReportItemDto>> GetSalesReportAsync(DateTime from, DateTime to, int? customerId = null)
    {
        var q = _uow.SalesInvoices.GetQueryable().Include(i => i.Customer)
            .Where(i => !i.IsDeleted && i.InvoiceDate >= from && i.InvoiceDate <= to.AddDays(1) && !i.IsHold);
        if (customerId.HasValue) q = q.Where(i => i.CustomerId == customerId);
        return _mapper.Map<IEnumerable<SalesReportItemDto>>(await q.OrderByDescending(i => i.InvoiceDate).ToListAsync());
    }

    public async Task<IEnumerable<StockReportItemDto>> GetStockReportAsync(int? categoryId = null, bool? lowStockOnly = null)
    {
        var stocks = await _uow.Stocks.GetWithDetailsAsync();
        if (categoryId.HasValue)
            stocks = stocks.Where(s => s.ProductVariant.Product.CategoryId == categoryId.Value);
        if (lowStockOnly == true)
            stocks = stocks.Where(s => s.Quantity <= s.ProductVariant.Product.ReorderPoint);
        return _mapper.Map<IEnumerable<StockReportItemDto>>(stocks);
    }

    public async Task<IEnumerable<CustomerLedgerDto>> GetCustomerLedgerAsync(int customerId, DateTime from, DateTime to)
        => _mapper.Map<IEnumerable<CustomerLedgerDto>>(await _uow.CustomerLedgers.GetByCustomerIdAsync(customerId, from, to));

    public async Task<IEnumerable<SupplierLedgerDto>> GetSupplierLedgerAsync(int supplierId, DateTime from, DateTime to)
        => _mapper.Map<IEnumerable<SupplierLedgerDto>>(await _uow.SupplierLedgers.GetBySupplierIdAsync(supplierId, from, to));

    public async Task<ProfitLossDto> GetProfitLossAsync(DateTime from, DateTime to)
    {
        var invoices = (await _uow.SalesInvoices.GetByDateRangeAsync(from, to))
            .Where(i => i.Status != InvoiceStatus.Cancelled).ToList();
        var totalSales = invoices.Sum(i => i.TotalAmount);
        var expenses = await _uow.AccountTransactions.GetTotalExpenseAsync(from, to);
        var expByCategory = (await _uow.AccountTransactions.GetByTypeAsync(TransactionType.Expense, from, to))
            .GroupBy(t => t.Category.ToString())
            .Select(g => new ExpenseSummaryDto { Category = g.Key, Amount = g.Sum(t => t.Amount) }).ToList();

        return new ProfitLossDto
        {
            FromDate = from,
            ToDate = to,
            TotalSales = totalSales,
            NetSales = totalSales,
            GrossProfit = totalSales,
            TotalExpenses = expenses,
            NetProfit = totalSales - expenses,
            ExpenseBreakdown = expByCategory
        };
    }

    public async Task<IEnumerable<CustomerDueDto>> GetCustomerDueListAsync()
    {
        var customers = await _uow.Customers.GetWithDueBalanceAsync();
        return customers.Select(c => new CustomerDueDto
        {
            CustomerId = c.Id,
            CustomerName = c.Name,
            PhoneNumber = c.PhoneNumber,
            GroupName = c.CustomerGroup.Name,
            DueAmount = c.CurrentBalance
        });
    }

    public async Task<IEnumerable<SupplierDueDto>> GetSupplierDueListAsync()
    {
        var suppliers = await _uow.Suppliers.GetWithDueBalanceAsync();
        return suppliers.Select(s => new SupplierDueDto
        {
            SupplierId = s.Id,
            CompanyName = s.CompanyName,
            PhoneNumber = s.PhoneNumber,
            DueAmount = s.CurrentBalance
        });
    }

    public async Task<IEnumerable<SalesReturnReportItemDto>> GetReturnReportAsync(DateTime from, DateTime to)
    {
        var returns = await _uow.SalesReturns.GetByDateRangeAsync(from, to);
        return _mapper.Map<IEnumerable<SalesReturnReportItemDto>>(returns);
    }

    public async Task<IEnumerable<PurchaseReportItemDto>> GetPurchaseReportAsync(DateTime from, DateTime to, int? supplierId = null)
    {
        var orders = await _uow.PurchaseOrders.GetByDateRangeAsync(from, to);
        if (supplierId.HasValue) orders = orders.Where(po => po.SupplierId == supplierId.Value);
        return _mapper.Map<IEnumerable<PurchaseReportItemDto>>(orders);
    }

    public async Task<decimal> GetSalesReturnsTotalAsync(DateTime from, DateTime to)
    {
        var returns = await _uow.SalesReturns.GetByDateRangeAsync(from, to);
        return returns.Sum(r => r.RefundAmount);
    }

    public async Task<decimal> GetCOGSAsync(DateTime from, DateTime to)
    {
        var invoices = await _uow.SalesInvoices.GetByDateRangeAsync(from, to);
        var invoicesList = invoices.Where(i => i.Status != InvoiceStatus.Cancelled).ToList();

        decimal totalCogs = 0;

        foreach (var invoice in invoicesList)
        {
            var invoiceDetails = await _uow.SalesInvoices.GetWithDetailsAsync(invoice.Id);
            if(invoiceDetails != null)
            {
               foreach(var item in invoiceDetails.Items) 
               {
                   totalCogs += item.Quantity * item.ProductVariant.Product.CostPrice;
               }
            }
        }

        return totalCogs;
    }
}