namespace ClothingERP.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ISalesService _sales;
    private readonly IStockService _stock;

    public DashboardService(IUnitOfWork uow, IMapper mapper, ISalesService sales, IStockService stock)
        => (_uow, _mapper, _sales, _stock) = (uow, mapper, sales, stock);

    public async Task<DashboardDto> GetDashboardDataAsync(int? branchId = null)
    {
        var lowStock = await _uow.Stocks.GetLowStockAsync(branchId);
        var outOfStock = await _uow.Stocks.GetOutOfStockAsync(branchId);

        var recentInvoicesQuery = _uow.SalesInvoices.GetQueryable()
            .Include(i => i.Customer).Where(i => !i.IsDeleted && !i.IsHold);
        if (branchId.HasValue)
            recentInvoicesQuery = recentInvoicesQuery.Where(i => i.BranchId == branchId.Value);

        var recentInvoices = await recentInvoicesQuery
            .OrderByDescending(i => i.InvoiceDate).Take(5).ToListAsync();

        var monthlySalesData = await _uow.SalesInvoices.GetMonthlySalesAsync(DateTime.Now.Year, branchId);

        // Top selling products (last 30 days)
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var topProductsQuery = _uow.SalesInvoices.GetQueryable()
            .Include(i => i.Items).ThenInclude(x => x.ProductVariant).ThenInclude(v => v.Product)
            .Where(i => !i.IsDeleted && i.InvoiceDate >= thirtyDaysAgo && i.Status != InvoiceStatus.Cancelled);
        if (branchId.HasValue)
            topProductsQuery = topProductsQuery.Where(i => i.BranchId == branchId.Value);

        var topProducts = await topProductsQuery
            .SelectMany(i => i.Items)
            .GroupBy(item => new { item.ProductVariant.Product.Name, item.ProductVariant.Product.SKU })
            .Select(g => new TopProductDto
            {
                ProductName = g.Key.Name,
                SKU = g.Key.SKU,
                QuantitySold = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.TotalAmount)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(5)
            .ToListAsync();

        var totalCustomerDue = await _uow.Customers.GetQueryable().Where(c => !c.IsDeleted).SumAsync(c => c.CurrentBalance);
        var totalSupplierDue = await _uow.Suppliers.GetQueryable().Where(s => !s.IsDeleted).SumAsync(s => s.CurrentBalance);
        var totalActiveCustomers = await _uow.Customers.CountAsync(c => c.IsActive && !c.IsDeleted);

        return new DashboardDto
        {
            TodaySales = await _sales.GetTodaySalesAsync(branchId),
            TodayProfit = await _sales.GetTodayProfitAsync(branchId),
            TodayInvoiceCount = await _sales.GetTodayInvoiceCountAsync(branchId),
            LowStockCount = lowStock.Count(),
            OutOfStockCount = outOfStock.Count(),
            TotalCustomerDue = totalCustomerDue,
            TotalSupplierDue = totalSupplierDue,
            TotalStockValue = await _stock.GetTotalStockValueAsync(branchId),
            TotalActiveCustomers = totalActiveCustomers,
            MonthlySalesChart = monthlySalesData.Select(m => new MonthlySalesChartDto
            { Month = m.Month, SalesAmount = m.SalesAmount, ProfitAmount = m.ProfitAmount }).ToList(),
            TopSellingProducts = topProducts,
            RecentInvoices = _mapper.Map<List<SalesInvoiceListDto>>(recentInvoices),
            LowStockAlerts = _mapper.Map<List<StockListDto>>(lowStock.Take(5))
        };
    }
}