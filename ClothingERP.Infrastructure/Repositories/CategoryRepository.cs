namespace ClothingERP.Infrastructure.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Category>> GetActiveWithSubCategoriesAsync()
        => await _dbSet.Include(c => c.SubCategories.Where(s => s.IsActive && !s.IsDeleted))
                       .Where(c => c.IsActive)
                       .OrderBy(c => c.Name)
                       .ToListAsync();

    public async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
    {
        var q = _dbSet.Where(c => c.Name.ToLower() == name.ToLower());
        if (excludeId.HasValue) q = q.Where(c => c.Id != excludeId.Value);
        return await q.AnyAsync();
    }
}