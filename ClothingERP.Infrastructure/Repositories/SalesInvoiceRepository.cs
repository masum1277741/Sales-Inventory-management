namespace ClothingERP.Infrastructure.Repositories;

public class SalesInvoiceRepository : GenericRepository<SalesInvoice>, ISalesInvoiceRepository
{
    public SalesInvoiceRepository(ApplicationDbContext context) : base(context) { }

    public async Task<SalesInvoice?> GetWithDetailsAsync(int id)
        => await _dbSet
            .Include(i => i.Customer)
            .Include(i => i.Items.Where(x => !x.IsDeleted))
                .ThenInclude(x => x.ProductVariant).ThenInclude(v => v.Product)
            .Include(i => i.Items.Where(x => !x.IsDeleted))
                .ThenInclude(x => x.ProductVariant).ThenInclude(v => v.Size)
            .Include(i => i.Items.Where(x => !x.IsDeleted))
                .ThenInclude(x => x.ProductVariant).ThenInclude(v => v.Color)
            .Include(i => i.Payments.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<IEnumerable<SalesInvoice>> GetByCustomerAsync(int customerId)
        => await _dbSet.Include(i => i.Customer)
                       .Where(i => i.CustomerId == customerId)
                       .OrderByDescending(i => i.InvoiceDate).ToListAsync();

    public async Task<IEnumerable<SalesInvoice>> GetByDateRangeAsync(DateTime from, DateTime to)
        => await _dbSet.Include(i => i.Customer)
                       .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to.AddDays(1))
                       .OrderByDescending(i => i.InvoiceDate).ToListAsync();

    public async Task<IEnumerable<SalesInvoice>> GetHeldInvoicesAsync()
        => await _dbSet.Include(i => i.Customer)
                       .Where(i => i.IsHold)
                       .OrderByDescending(i => i.InvoiceDate).ToListAsync();

    public async Task<string> GenerateInvoiceNumberAsync()
    {
        var today = DateTime.Now;
        var count = await _dbSet.CountAsync(i => i.InvoiceDate.Date == today.Date) + 1;
        return $"INV-{today:yyyyMMdd}-{count:D4}";
    }

    public async Task<decimal> GetTodaySalesAmountAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _dbSet
            .Where(i => i.InvoiceDate >= today && i.Status != InvoiceStatus.Cancelled && !i.IsHold)
            .SumAsync(i => i.TotalAmount);
    }

    public async Task<decimal> GetTodayProfitAsync()
    {
        var today = DateTime.UtcNow.Date;
        var items = await _context.Set<SalesInvoiceItem>()
            .Include(x => x.SalesInvoice)
            .Include(x => x.ProductVariant).ThenInclude(v => v.Product)
            .Where(x => !x.IsDeleted &&
                        !x.SalesInvoice.IsDeleted &&
                        x.SalesInvoice.InvoiceDate >= today &&
                        x.SalesInvoice.Status != InvoiceStatus.Cancelled &&
                        !x.SalesInvoice.IsHold)
            .ToListAsync();

        return items.Sum(x =>
            (x.UnitPrice - (x.ProductVariant.CostPriceOverride ?? x.ProductVariant.Product.CostPrice))
            * x.Quantity - x.DiscountAmount);
    }

    public async Task<int> GetTodayInvoiceCountAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _dbSet.CountAsync(i => i.InvoiceDate >= today &&
                                            i.Status != InvoiceStatus.Cancelled && !i.IsHold);
    }

    public async Task<List<MonthlySalesData>> GetMonthlySalesAsync(int year)
    {
        var invoices = await _dbSet
            .Include(i => i.Items.Where(x => !x.IsDeleted))
                .ThenInclude(x => x.ProductVariant).ThenInclude(v => v.Product)
            .Where(i => i.InvoiceDate.Year == year && i.Status != InvoiceStatus.Cancelled && !i.IsHold)
            .ToListAsync();

        return invoices
            .GroupBy(i => i.InvoiceDate.Month)
            .Select(g => new MonthlySalesData(
                Month: CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key),
                MonthNumber: g.Key,
                SalesAmount: g.Sum(i => i.TotalAmount),
                ProfitAmount: g.Sum(i => i.Items.Sum(x =>
                    (x.UnitPrice - (x.ProductVariant.CostPriceOverride ?? x.ProductVariant.Product.CostPrice))
                    * x.Quantity - x.DiscountAmount))
            ))
            .OrderBy(m => m.MonthNumber)
            .ToList();
    }
}