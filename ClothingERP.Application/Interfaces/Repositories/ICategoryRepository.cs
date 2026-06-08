namespace ClothingERP.Application.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetActiveWithSubCategoriesAsync();
    Task<bool> IsNameExistsAsync(string name, int? excludeId = null);
}