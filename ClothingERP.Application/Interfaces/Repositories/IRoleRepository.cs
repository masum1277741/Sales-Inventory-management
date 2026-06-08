namespace ClothingERP.Application.Interfaces.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetWithPermissionsAsync(int roleId);
    Task<IEnumerable<Role>> GetActiveRolesAsync();
}