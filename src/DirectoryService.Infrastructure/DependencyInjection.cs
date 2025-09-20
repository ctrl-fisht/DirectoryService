using DirectoryService.Application.Database;
using DirectoryService.Application.Repositories;
using DirectoryService.Infrastructure.CleanupBackgroundService;
using DirectoryService.Infrastructure.Dapper;
using DirectoryService.Infrastructure.EfCore;
using DirectoryService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connString = configuration.GetConnectionString("Postgres");

        services.AddDbContext<AppDbContext>(options =>
        {  
            options.UseNpgsql(connString);
        });

        services.AddScoped<ITransactionManager, EfCoreTransactionManager>();
        services.AddScoped<IConnectionFactory, NpgsqlConnectionFactory>();
        
        services.AddScoped<ILocationsRepository, LocationsesRepository>();
        services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
        services.AddScoped<IPositionsRepository, PositionsRepository>();
        
        // Cleanup Departments Background Service
        services.AddHostedService<CleanupDepartmentsBackgroundService>();
        services.Configure<CleanupDepartmentsOptions>(configuration.GetSection("CleanupDepartmentsOptions"));
        
        return services;
    }
}