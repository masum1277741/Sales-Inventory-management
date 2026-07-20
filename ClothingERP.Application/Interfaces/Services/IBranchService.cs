namespace ClothingERP.Application.Interfaces.Services;

public interface IBranchService
{
    Task<IEnumerable<BranchDto>> GetAllAsync();
    Task<BranchDto?> GetByIdAsync(int id);
    Task<ServiceResult<BranchDto>> CreateAsync(CreateBranchDto dto, int userId);
    Task<ServiceResult> UpdateAsync(int id, CreateBranchDto dto, int userId);
    Task<ServiceResult> ToggleStatusAsync(int id, int userId);

    Task<MyBranchAccessDto> GetUserAccessAsync(int userId, string roleName);
    Task<int> GetUserDefaultBranchIdAsync(int userId);
    Task<ServiceResult> AssignUserToBranchesAsync(UserBranchAssignmentDto dto, int userId);

    Task<List<BranchStockComparisonDto>> CompareStockAcrossBranchesAsync(string? keyword = null);
}