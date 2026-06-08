namespace ClothingERP.Application.Interfaces.Services;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllAsync();
    Task<RoleDto?> GetByIdAsync(int id);
    Task<RoleWithPermissionsDto?> GetWithPermissionsAsync(int id);
    Task<IEnumerable<RolePermissionDto>> GetAllModulesWithPermissionsAsync(int roleId);
    Task<ServiceResult<RoleDto>> CreateAsync(CreateRoleDto dto, int createdBy);
    Task<ServiceResult<RoleDto>> UpdateAsync(int id, CreateRoleDto dto, int updatedBy);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SavePermissionsAsync(int roleId, List<SavePermissionDto> permissions, int updatedBy);
}