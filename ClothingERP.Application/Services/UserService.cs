namespace ClothingERP.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _audit;

    public UserService(IUnitOfWork uow, IMapper mapper, IAuditLogService audit)
        => (_uow, _mapper, _audit) = (uow, mapper, audit);

    public async Task<IEnumerable<UserListDto>> GetAllAsync()
    {
        var users = await _uow.Users.GetQueryable()
            .Include(u => u.Role)
            .Include(u => u.UserBranches).ThenInclude(ub => ub.Branch)
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return users.Select(u => new UserListDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Username = u.Username,
            RoleName = u.Role?.Name ?? "",
            RoleId = u.RoleId,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            PhoneNumber = u.PhoneNumber,
            BranchNames = u.UserBranches.Any()
                ? string.Join(", ", u.UserBranches.Where(ub => !ub.IsDeleted).Select(ub => ub.Branch.Name))
                : "—"
        });
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _uow.Users.GetWithRoleAsync(id);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<ServiceResult<UserDto>> CreateAsync(CreateUserDto dto, int createdBy)
    {
        if (await _uow.Users.IsUsernameExistsAsync(dto.Username))
            return ServiceResult<UserDto>.Fail("Username already exists.");
        if (await _uow.Users.IsEmailExistsAsync(dto.Email))
            return ServiceResult<UserDto>.Fail("Email already exists.");

        var user = _mapper.Map<User>(dto);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        user.CreatedBy = createdBy;

        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        await _audit.LogAsync(createdBy, "Create", "Users", user.Id.ToString(),
            newValues: JsonSerializer.Serialize(new { user.Username, user.Email, user.RoleId }));

        return ServiceResult<UserDto>.Ok(_mapper.Map<UserDto>(user), "User created successfully.");
    }

    public async Task<ServiceResult<UserDto>> UpdateAsync(int id, UpdateUserDto dto, int updatedBy)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user == null) return ServiceResult<UserDto>.Fail("User not found.");

        if (await _uow.Users.IsEmailExistsAsync(dto.Email, id))
            return ServiceResult<UserDto>.Fail("Email already used by another user.");

        var oldValues = JsonSerializer.Serialize(new { user.Email, user.RoleId, user.IsActive });

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;
        user.RoleId = dto.RoleId;
        user.IsActive = dto.IsActive;
        user.ProfileImage = dto.ProfileImagePath ?? user.ProfileImage;
        user.UpdatedBy = updatedBy;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        await _audit.LogAsync(updatedBy, "Update", "Users", id.ToString(),
            oldValues, JsonSerializer.Serialize(new { user.Email, user.RoleId, user.IsActive }));

        return ServiceResult<UserDto>.Ok(_mapper.Map<UserDto>(user), "User updated successfully.");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user == null) return ServiceResult.Fail("User not found.");
        _uow.Users.Remove(user);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("User deleted.");
    }

    public async Task<ServiceResult> ToggleStatusAsync(int id, int updatedBy)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user == null) return ServiceResult.Fail("User not found.");
        user.IsActive = !user.IsActive;
        user.UpdatedBy = updatedBy;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok(user.IsActive ? "User activated." : "User deactivated.");
    }

    public async Task<ServiceResult> ResetPasswordAsync(int id, string newPassword, int updatedBy)
    {
        var user = await _uow.Users.GetByIdAsync(id);
        if (user == null) return ServiceResult.Fail("User not found.");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedBy = updatedBy;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();
        return ServiceResult.Ok("Password reset successfully.");
    }
}