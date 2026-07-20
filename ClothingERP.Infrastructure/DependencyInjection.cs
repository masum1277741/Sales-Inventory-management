using ClothingERP.Application.Interfaces.Services;
using ClothingERP.Infrastructure.Repositories;
using ClothingERP.Infrastructure.Services;

namespace ClothingERP.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly("ClothingERP.Infrastructure")
            ));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
        return services;
    }
}