using System.Data.Common;
using DirectoryService.Infrastructure.EfCore;
using DirectoryService.Presentation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace DirectoryService.IntegrationTests;

public class DSWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres")
        .WithDatabase("directory_service")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private Respawner _respawner = null!;
    private DbConnection _dbConnection = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<AppDbContext>();
            
            services.AddDbContext<AppDbContext>(options =>
            {  
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });
        });
    }
    
    public async Task InitializeRespawner(DbConnection dbConnection)
    {
        _respawner = await Respawner.CreateAsync(
            dbConnection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"]
            });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }
    
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();
        await InitializeRespawner(_dbConnection);
    }

    public async Task DisposeAsync()
    {
        await _dbConnection.CloseAsync();
        await _dbConnection.DisposeAsync();
        
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}