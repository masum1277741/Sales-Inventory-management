namespace ClothingERP.Application.Interfaces.Repositories;

public interface IRolePermissionRepository : IRepository<RolePermission>
{
    Task<RolePermission?> GetByRoleAndModuleAsync(int roleId, int moduleId);
    Task<IEnumerable<RolePermission>> GetByRoleIdAsync(int roleId);
    Task<bool> HasPermissionAsync(int roleId, string controller, string action);
    Task DeleteByRoleIdAsync(int roleId);
}