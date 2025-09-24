using DirectoryService.Application.Database;
using DirectoryService.Application.Repositories;
using DirectoryService.Infrastructure.CleanupBackgroundService;
using DirectoryService.Infrastructure.Dapper;
using DirectoryService.Infrastructure.EfCore;
using DirectoryService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Caching;

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

        services.AddDistributedCache(configuration);

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

    public static IServiceCollection AddDistributedCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string redisString = configuration.GetConnectionString("Redis")
                             ?? throw new ArgumentNullException(nameof(redisString));
        
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisString;
        });

        services.AddSingleton<ICacheService, CacheService>();
        
        return services;
    }
}