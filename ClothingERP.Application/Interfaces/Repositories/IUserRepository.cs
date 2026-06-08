namespace ClothingERP.Application.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetWithRoleAsync(int userId);
    Task<bool> IsUsernameExistsAsync(string username, int? excludeId = null);
    Task<bool> IsEmailExistsAsync(string email, int? excludeId = null);
}