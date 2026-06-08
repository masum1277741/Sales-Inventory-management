namespace ClothingERP.Application.Interfaces.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);
    Task<ServiceResult<UserDto>> CreateAsync(CreateUserDto dto, int createdBy);
    Task<ServiceResult<UserDto>> UpdateAsync(int id, UpdateUserDto dto, int updatedBy);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> ToggleStatusAsync(int id, int updatedBy);
    Task<ServiceResult> ResetPasswordAsync(int id, string newPassword, int updatedBy);
}