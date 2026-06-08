namespace ClothingERP.Application.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public RoleService(IUnitOfWork uow, IMapper mapper)
        => (_uow, _mapper) = (uow, mapper);

    public async Task<IEnumerable<RoleDto>> GetAllAsync()
    {
        var roles = await _uow.Roles.GetQueryable()
            .Include(r => r.Users.Where(u => !u.IsDeleted))
            .Where(r => !r.IsDeleted).ToListAsync();
        return _mapper.Map<IEnumerable<RoleDto>>(roles);
    }

    public async Task<RoleDto?> GetByIdAsync(int id)
    {
        var role = await _uow.Roles.GetByIdAsync(id);
        return role == null ? null : _mapper.Map<RoleDto>(role);
    }

    public async Task<RoleWithPermissionsDto?> GetWithPermissionsAsync(int id)
    {
        var role = await _uow.Roles.GetWithPermissionsAsync(id);
        return role == null ? null : _mapper.Map<RoleWithPermissionsDto>(role);
    }

    public async Task<IEnumerable<RolePermissionDto>> GetAllModulesWithPermissionsAsync(int roleId)
    {
        var modules = await _uow.AppModules.GetActiveModulesAsync();
        var permissions = await _uow.RolePermissions.GetByRoleIdAsync(roleId);
        var permDict = permissions.ToDictionary(p => p.ModuleId);

        return modules.Select(m => permDict.TryGetValue(m.Id, out var perm)
            ? _mapper.Map<RolePermissionDto>(perm)
            : new RolePermissionDto { ModuleId = m.Id, ModuleName = m.Name, ModuleIcon = m.Icon, ParentModuleId = m.ParentModuleId }).ToList();
    }

    public async Task<ServiceResult<RoleDto>> CreateAsync(CreateRoleDto dto, int createdBy)
    {
        if (await _uow.Roles.AnyAsync(r => r.Name.ToLower() == dto.Name.ToLower()))
            return ServiceResult<RoleDto>.Fail("Role name already exists.");

        var role = _mapper.Map<Role>(dto);
        role.CreatedBy = createdBy;
        await _uow.Roles.AddAsync(role);
        await _uow.SaveChangesAsync();
        return ServiceResult<RoleDto>.Ok(_mapper.Map<RoleDto>(role), "Role created.");
    }

    public async Task<ServiceResult<RoleDto>> UpdateAsync(int id, CreateRoleDto dto, int updatedBy)
    {
        var role = await _uow.Roles.GetByIdAsync(id);
        if (role == null) return ServiceResult<RoleDto>.Fail("Role not found.");
        if (await _uow.Roles.AnyAsync(r => r.Name.ToLower() == dto.Name.ToLower() && r.Id != id))
            return ServiceResult<RoleDto>.Fail("Role name already exists.");

        role.Name = dto.Name;
        role.Description = dto.Description;
        role.IsActive = dto.IsActive;
        role.UpdatedBy = updatedBy;
        _uow.Roles.Update(role);
        await _uow.SaveChangesAsync();
        return ServiceResult<RoleDto>.Ok(_mapper.Map<RoleDto>(role), "Role updated.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var role = await _uow.Roles.GetByIdAsync(id);
        if (role == null) return ServiceResult.Fail("Role not found.");
        if (await _uow.Users.AnyAsync(u => u.RoleId == id && !u.IsDeleted))
            return ServiceResult.Fail("Cannot delete role with active users.");
        _uow.Roles.Remove(role);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Role deleted.");
    }

    public async Task<ServiceResult> SavePermissionsAsync(int roleId, List<SavePermissionDto> permissions, int updatedBy)
    {
        await _uow.RolePermissions.DeleteByRoleIdAsync(roleId);
        await _uow.SaveChangesAsync();

        var newPerms = permissions.Select(p => new RolePermission
        {
            RoleId = roleId,
            ModuleId = p.ModuleId,
            CanView = p.CanView,
            CanInsert = p.CanInsert,
            CanUpdate = p.CanUpdate,
            CanDelete = p.CanDelete,
            CanPrint = p.CanPrint,
            CanExport = p.CanExport,
            CreatedBy = updatedBy
        }).ToList();

        await _uow.RolePermissions.AddRangeAsync(newPerms);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Permissions saved.");
    }
}