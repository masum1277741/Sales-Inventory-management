namespace ClothingERP.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditSvc;

    public AuthService(IUnitOfWork uow, IMapper mapper, IAuditLogService auditSvc)
        => (_uow, _mapper, _auditSvc) = (uow, mapper, auditSvc);


    public async Task<ServiceResult<UserDto>> LoginAsync(LoginDto dto, string ipAddress, string userAgent)
    {
        var user = await _uow.Users.GetByUsernameAsync(dto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
         
            if (user != null)
                await _auditSvc.LogAsync(user.Id, "LoginFailed", "Users",
                    user.Id.ToString(), ipAddress: ipAddress,
                    description: $"Failed login attempt for '{user.Username}'");

            return ServiceResult<UserDto>.Fail("Invalid username or password.");
        }

        if (!user.IsActive)
            return ServiceResult<UserDto>.Fail("Account is inactive.");

        user.LastLoginAt = DateTime.Now;
        await _uow.SaveChangesAsync();


        await _auditSvc.LogAsync(user.Id, "LoginSuccess", "Users",
            user.Id.ToString(), ipAddress: ipAddress,
            description: $"User '{user.Username}' logged in successfully");

        return ServiceResult<UserDto>.Ok(_mapper.Map<UserDto>(user));
    }

    public async Task LogoutAsync(int userId, string ipAddress)
        
        => await _auditSvc.LogAsync(userId, "Logout", "Users",
               userId.ToString(), ipAddress: ipAddress);

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

   
        await _auditSvc.LogAsync(userId, "ChangePassword", "Users", userId.ToString());
        return ServiceResult.Ok("Password changed successfully.");
    }

    public async Task<UserDto?> GetCurrentUserAsync(int userId)
    {
        var user = await _uow.Users.GetWithRoleAsync(userId);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }
}