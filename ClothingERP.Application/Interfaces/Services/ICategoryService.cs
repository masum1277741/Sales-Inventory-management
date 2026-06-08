namespace ClothingERP.Application.Interfaces.Services;

public interface ICategoryService
{
    // Category
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<ServiceResult<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto, int userId);
    Task<ServiceResult<CategoryDto>> UpdateCategoryAsync(int id, CreateCategoryDto dto, int userId);
    Task<ServiceResult> DeleteCategoryAsync(int id);
    Task<ServiceResult> ToggleCategoryStatusAsync(int id, int userId);

    // SubCategory
    Task<IEnumerable<SubCategoryDto>> GetSubCategoriesAsync(int? categoryId = null);
    Task<SubCategoryDto?> GetSubCategoryByIdAsync(int id);
    Task<ServiceResult<SubCategoryDto>> CreateSubCategoryAsync(CreateSubCategoryDto dto, int userId);
    Task<ServiceResult<SubCategoryDto>> UpdateSubCategoryAsync(int id, CreateSubCategoryDto dto, int userId);
    Task<ServiceResult> DeleteSubCategoryAsync(int id);
}