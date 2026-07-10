using ClothingERP.Infrastructure.Repositories;

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

        return services;
    }
}