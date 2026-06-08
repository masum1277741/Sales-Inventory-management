using System.Reflection;
using ClothingERP.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ClothingERP.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(config => { }, Assembly.GetExecutingAssembly());

        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IBarcodeService, BarcodeService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IProductAttributeService, ProductAttributeService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<ISalesService, SalesService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<IReturnService, ReturnService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}