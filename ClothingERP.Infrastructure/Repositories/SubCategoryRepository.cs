namespace ClothingERP.Infrastructure.Repositories;

public class SubCategoryRepository : GenericRepository<SubCategory>, ISubCategoryRepository
{
    public SubCategoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<SubCategory>> GetByCategoryIdAsync(int categoryId)
        => await _dbSet.Where(s => s.CategoryId == categoryId && s.IsActive)
                       .OrderBy(s => s.Name)
                       .ToListAsync();

    public async Task<bool> IsNameExistsAsync(string name, int categoryId, int? excludeId = null)
    {
        var q = _dbSet.Where(s => s.Name.ToLower() == name.ToLower() && s.CategoryId == categoryId);
        if (excludeId.HasValue) q = q.Where(s => s.Id != excludeId.Value);
        return await q.AnyAsync();
    }
}