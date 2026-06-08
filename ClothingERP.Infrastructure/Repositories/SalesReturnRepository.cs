namespace ClothingERP.Infrastructure.Repositories;

public class SalesReturnRepository : GenericRepository<SalesReturn>, ISalesReturnRepository
{
    public SalesReturnRepository(ApplicationDbContext context) : base(context) { }

    public async Task<SalesReturn?> GetWithDetailsAsync(int id)
        => await _dbSet
            .Include(r => r.SalesInvoice)
            .Include(r => r.Customer)
            .Include(r => r.Items.Where(x => !x.IsDeleted))
                .ThenInclude(x => x.ProductVariant).ThenInclude(v => v.Product)
            .Include(r => r.Items.Where(x => !x.IsDeleted))
                .ThenInclude(x => x.ProductVariant).ThenInclude(v => v.Size)
            .Include(r => r.Items.Where(x => !x.IsDeleted))
                .ThenInclude(x => x.ProductVariant).ThenInclude(v => v.Color)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<SalesReturn>> GetByInvoiceIdAsync(int invoiceId)
        => await _dbSet.Where(r => r.SalesInvoiceId == invoiceId)
                       .OrderByDescending(r => r.ReturnDate).ToListAsync();

    public async Task<IEnumerable<SalesReturn>> GetByDateRangeAsync(DateTime from, DateTime to)
        => await _dbSet.Include(r => r.Customer).Include(r => r.SalesInvoice)
                       .Where(r => r.ReturnDate >= from && r.ReturnDate <= to.AddDays(1))
                       .OrderByDescending(r => r.ReturnDate).ToListAsync();

    public async Task<string> GenerateReturnNumberAsync()
    {
        var today = DateTime.Now;
        var count = await _dbSet.CountAsync(r => r.ReturnDate.Date == today.Date) + 1;
        return $"SR-{today:yyyyMMdd}-{count:D4}";
    }
}