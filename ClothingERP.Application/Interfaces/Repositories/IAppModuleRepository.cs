namespace ClothingERP.Application.Interfaces.Repositories;

public interface IAppModuleRepository : IRepository<AppModule>
{
    Task<IEnumerable<AppModule>> GetMenuTreeAsync();
    Task<IEnumerable<AppModule>> GetActiveModulesAsync();
    Task<IEnumerable<AppModule>> GetParentModulesAsync();
}