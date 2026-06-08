namespace ClothingERP.Infrastructure.Repositories;

public class RolePermissionRepository : GenericRepository<RolePermission>, IRolePermissionRepository
{
    public RolePermissionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<RolePermission?> GetByRoleAndModuleAsync(int roleId, int moduleId)
        => await _dbSet.FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.ModuleId == moduleId);

    public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(int roleId)
        => await _dbSet.Include(rp => rp.Module)
                       .Where(rp => rp.RoleId == roleId)
                       .OrderBy(rp => rp.Module.SortOrder)
                       .ToListAsync();

    public async Task<bool> HasPermissionAsync(int roleId, string controller, string action)
        => await _dbSet.AnyAsync(rp =>
               rp.RoleId == roleId &&
               rp.Module.Controller == controller &&
               rp.Module.Action == action &&
               rp.CanView);

    public async Task DeleteByRoleIdAsync(int roleId)
    {
        var perms = await _dbSet.Where(rp => rp.RoleId == roleId).ToListAsync();
        foreach (var p in perms) { p.IsDeleted = true; p.UpdatedAt = DateTime.UtcNow; }
        _dbSet.UpdateRange(perms);
    }
}