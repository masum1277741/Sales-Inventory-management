namespace ClothingERP.Infrastructure.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Role?> GetWithPermissionsAsync(int roleId)
        => await _dbSet.Include(r => r.RolePermissions)
                           .ThenInclude(rp => rp.Module)
                       .FirstOrDefaultAsync(r => r.Id == roleId);

    public async Task<IEnumerable<Role>> GetActiveRolesAsync()
        => await _dbSet.Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync();
}