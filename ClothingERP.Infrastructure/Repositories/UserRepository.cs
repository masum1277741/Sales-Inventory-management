namespace ClothingERP.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username)
        => await _dbSet.Include(u => u.Role)
                       .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

    public async Task<User?> GetByEmailAsync(string email)
        => await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<User?> GetWithRoleAsync(int userId)
        => await _dbSet.Include(u => u.Role)
                       .FirstOrDefaultAsync(u => u.Id == userId);

    public async Task<bool> IsUsernameExistsAsync(string username, int? excludeId = null)
    {
        var q = _dbSet.Where(u => u.Username.ToLower() == username.ToLower());
        if (excludeId.HasValue) q = q.Where(u => u.Id != excludeId.Value);
        return await q.AnyAsync();
    }

    public async Task<bool> IsEmailExistsAsync(string email, int? excludeId = null)
    {
        var q = _dbSet.Where(u => u.Email.ToLower() == email.ToLower());
        if (excludeId.HasValue) q = q.Where(u => u.Id != excludeId.Value);
        return await q.AnyAsync();
    }
}