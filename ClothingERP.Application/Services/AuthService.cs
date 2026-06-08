namespace ClothingERP.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _audit;

    public AuthService(IUnitOfWork uow, IMapper mapper, IAuditLogService audit)
        => (_uow, _mapper, _audit) = (uow, mapper, audit);

    public async Task<ServiceResult<UserDto>> LoginAsync(LoginDto dto, string ip, string ua)
    {
        var user = await _uow.Users.GetByUsernameAsync(dto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return ServiceResult<UserDto>.Fail("Invalid username or password.");

        if (!user.IsActive)
            return ServiceResult<UserDto>.Fail("Your account is inactive. Contact administrator.");

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        await _audit.LogAsync(user.Id, "Login", "Users", user.Id.ToString(), ipAddress: ip);

        return ServiceResult<UserDto>.Ok(_mapper.Map<UserDto>(user), "Login successful.");
    }

    public async Task LogoutAsync(int userId, string ip)
        => await _audit.LogAsync(userId, "Logout", "Users", userId.ToString(), ipAddress: ip);

    public async Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null) return ServiceResult.Fail("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return ServiceResult.Fail("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = userId;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        await _audit.LogAsync(userId, "ChangePassword", "Users", userId.ToString());
        return ServiceResult.Ok("Password changed successfully.");
    }

    public async Task<UserDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _uow.Users.GetWithRoleAsync(userId);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }
}