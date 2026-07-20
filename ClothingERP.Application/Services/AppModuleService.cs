//namespace ClothingERP.Application.Services;

//public class AppModuleService : IAppModuleService
//{
//    private readonly IUnitOfWork _uow;

//    public AppModuleService(IUnitOfWork uow) => _uow = uow;

//    public async Task<IEnumerable<AppModuleDto>> GetAllAsync()
//    {
//        var modules = await _uow.AppModules.GetQueryable()
//            .Where(m => !m.IsDeleted && m.IsActive)
//            .OrderBy(m => m.GroupName)
//            .ThenBy(m => m.DisplayOrder)
//            .ToListAsync();

//        return modules.Select(m => new AppModuleDto
//        {
//            Id = m.Id,
//            Name = m.Name,
//            GroupName = m.GroupName,
//            Icon = m.Icon,
//            Controller = m.ControllerName,
//            Action = m.ActionName,
//            DisplayOrder = m.DisplayOrder,
//            IsActive = m.IsActive
//        });
//    }

//    public async Task<List<int>> GetUserModuleIdsAsync(int userId)
//    {
//        var user = await _uow.Users.GetQueryable()
//            .Include(u => u.Role).ThenInclude(r => r.RoleModules)
//            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

//        if (user == null) return new List<int>();

//        // User নিজের overridden module list থাকলে সেটা নেবে,
//        // নাহলে Role এর default module list থেকে নেবে
//        var userSpecificModules = await _uow.UserModules.GetQueryable()
//            .Where(um => um.UserId == userId && !um.IsDeleted)
//            .ToListAsync();

//        if (userSpecificModules.Any())
//            return userSpecificModules.Select(m => m.AppModuleId).ToList();

//        return user.Role?.RoleModules?
//            .Where(rm => !rm.IsDeleted)
//            .Select(rm => rm.AppModuleId)
//            .ToList() ?? new List<int>();
//    }

//    public async Task<ServiceResult> SaveUserPermissionsAsync(int userId, List<int> moduleIds, int updatedBy)
//    {
//        // পুরনো user-specific permissions সরাও
//        var existing = await _uow.UserModules.GetQueryable()
//            .Where(um => um.UserId == userId && !um.IsDeleted)
//            .ToListAsync();

//        foreach (var e in existing) _uow.UserModules.Remove(e);
//        await _uow.SaveChangesAsync();

//        // নতুন permissions সেট করো
//        foreach (var moduleId in moduleIds)
//        {
//            await _uow.UserModules.AddAsync(new UserModule
//            {
//                UserId = userId,
//                AppModuleId = moduleId,
//                CreatedBy = updatedBy
//            });
//        }
//        await _uow.SaveChangesAsync();

//        return ServiceResult.Ok($"{moduleIds.Count} টা menu permission সেভ হয়েছে।");
//    }
//}