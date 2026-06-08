namespace ClothingERP.Infrastructure.Repositories;

public class GoodsReceiptNoteRepository : GenericRepository<GoodsReceiptNote>, IGoodsReceiptNoteRepository
{
    public GoodsReceiptNoteRepository(ApplicationDbContext context) : base(context) { }

    public async Task<GoodsReceiptNote?> GetWithDetailsAsync(int id)
        => await _dbSet
            .Include(g => g.PurchaseOrder)
            .Include(g => g.Supplier)
            .Include(g => g.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product)
            .Include(g => g.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Size)
            .Include(g => g.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Color)
            .FirstOrDefaultAsync(g => g.Id == id);

    public async Task<IEnumerable<GoodsReceiptNote>> GetByPurchaseOrderAsync(int purchaseOrderId)
        => await _dbSet.Include(g => g.Supplier)
                       .Where(g => g.PurchaseOrderId == purchaseOrderId)
                       .OrderByDescending(g => g.ReceivedDate).ToListAsync();

    public async Task<string> GenerateGRNNumberAsync()
    {
        var year = DateTime.Now.Year;
        var month = DateTime.Now.Month;
        var count = await _dbSet.CountAsync(g => g.ReceivedDate.Year == year && g.ReceivedDate.Month == month) + 1;
        return $"GRN-{year}{month:D2}-{count:D4}";
    }
}