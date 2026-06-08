namespace ClothingERP.Infrastructure.Repositories;

public class PurchaseOrderRepository : GenericRepository<PurchaseOrder>, IPurchaseOrderRepository
{
    public PurchaseOrderRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PurchaseOrder?> GetWithDetailsAsync(int id)
        => await _dbSet
            .Include(po => po.Supplier)
            .Include(po => po.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product)
            .Include(po => po.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Size)
            .Include(po => po.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Color)
            .FirstOrDefaultAsync(po => po.Id == id);

    public async Task<IEnumerable<PurchaseOrder>> GetBySupplierAsync(int supplierId)
        => await _dbSet.Include(po => po.Supplier)
                       .Where(po => po.SupplierId == supplierId)
                       .OrderByDescending(po => po.OrderDate).ToListAsync();

    public async Task<IEnumerable<PurchaseOrder>> GetByStatusAsync(PurchaseOrderStatus status)
        => await _dbSet.Include(po => po.Supplier)
                       .Where(po => po.Status == status)
                       .OrderByDescending(po => po.OrderDate).ToListAsync();

    public async Task<IEnumerable<PurchaseOrder>> GetByDateRangeAsync(DateTime from, DateTime to)
        => await _dbSet.Include(po => po.Supplier)
                       .Where(po => po.OrderDate >= from && po.OrderDate <= to.AddDays(1))
                       .OrderByDescending(po => po.OrderDate).ToListAsync();

    public async Task<string> GeneratePONumberAsync()
    {
        var year = DateTime.Now.Year;
        var month = DateTime.Now.Month;
        var count = await _dbSet.CountAsync(p => p.OrderDate.Year == year && p.OrderDate.Month == month) + 1;
        return $"PO-{year}{month:D2}-{count:D4}";
    }
}