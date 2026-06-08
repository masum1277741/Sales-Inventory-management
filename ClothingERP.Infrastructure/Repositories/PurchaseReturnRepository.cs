namespace ClothingERP.Infrastructure.Repositories;

public class PurchaseReturnRepository : GenericRepository<PurchaseReturn>, IPurchaseReturnRepository
{
    public PurchaseReturnRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PurchaseReturn?> GetWithDetailsAsync(int id)
        => await _dbSet
            .Include(r => r.PurchaseOrder)
            .Include(r => r.Supplier)
            .Include(r => r.Items.Where(x => !x.IsDeleted))
                .ThenInclude(x => x.ProductVariant).ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<PurchaseReturn>> GetByDateRangeAsync(DateTime from, DateTime to)
        => await _dbSet.Include(r => r.Supplier).Include(r => r.PurchaseOrder)
                       .Where(r => r.ReturnDate >= from && r.ReturnDate <= to.AddDays(1))
                       .OrderByDescending(r => r.ReturnDate).ToListAsync();

    public async Task<string> GenerateReturnNumberAsync()
    {
        var today = DateTime.Now;
        var count = await _dbSet.CountAsync(r => r.ReturnDate.Date == today.Date) + 1;
        return $"PR-{today:yyyyMMdd}-{count:D4}";
    }
}