namespace ClothingERP.Application.Interfaces.Repositories;

public interface ISubCategoryRepository : IRepository<SubCategory>
{
    Task<IEnumerable<SubCategory>> GetByCategoryIdAsync(int categoryId);
    Task<bool> IsNameExistsAsync(string name, int categoryId, int? excludeId = null);
}