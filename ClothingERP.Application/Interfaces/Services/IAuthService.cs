namespace ClothingERP.Application.Interfaces.Services;

public interface IAuthService
{
    Task<ServiceResult<UserDto>> LoginAsync(LoginDto dto, string ipAddress, string userAgent);
    Task LogoutAsync(int userId, string ipAddress);
    Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task<UserDto?> GetCurrentUserAsync(int userId);
}