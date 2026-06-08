namespace ClothingERP.Infrastructure.Repositories;

public class AppModuleRepository : GenericRepository<AppModule>, IAppModuleRepository
{
    public AppModuleRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<AppModule>> GetMenuTreeAsync()
        => await _dbSet.Include(m => m.ChildModules)
                       .Where(m => m.ParentModuleId == null && m.IsActive)
                       .OrderBy(m => m.SortOrder)
                       .ToListAsync();

    public async Task<IEnumerable<AppModule>> GetActiveModulesAsync()
        => await _dbSet.Where(m => m.IsActive).OrderBy(m => m.SortOrder).ToListAsync();

    public async Task<IEnumerable<AppModule>> GetParentModulesAsync()
        => await _dbSet.Where(m => m.ParentModuleId == null && m.IsActive)
                       .OrderBy(m => m.SortOrder).ToListAsync();
}