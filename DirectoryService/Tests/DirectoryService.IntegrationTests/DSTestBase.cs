using DirectoryService.Domain.Entities;
using DirectoryService.Domain.ValueObjects;
using DirectoryService.Infrastructure.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests;

public class DSTestBase : IClassFixture<DSWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;

    protected IServiceProvider Services;

    public DSTestBase(DSWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _resetDatabase();

    protected async Task<T> ExecuteInDb<T>(Func<AppDbContext, Task<T>> func)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await func(dbContext);
    }

    protected async Task<T> ExecuteHandlerAsync<THandler, T>(Func<THandler, Task<T>> func)
    {
        using var scope = Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<THandler>();
        return await func(handler);
    }
    
    
    protected async Task<Guid> CreateOneLocation()
    {
        var timezone = Timezone.Create("Europe/Moscow");
        var name = LocationName.Create("Москва-Сити офис");
        var address = Address.Create(
            "Россия", "Москва", "Тверская", "1", 15);
        var location = Location.Create(name.Value, address.Value, timezone.Value);

        return await ExecuteInDb<Guid>(async (dbContext) =>
        {
            await dbContext.Locations.AddAsync(location.Value, CancellationToken.None);
            await dbContext.SaveChangesAsync(CancellationToken.None);
            return location.Value.Id;
        });
    }
    
    // Создаётся в БД БЕЗ ЛОКАЦИЙ
    protected async Task<Guid> CreateOneDepartment()
    {
        var name = DepartmentName.Create("Тестовый офис");
        var identifier = Identifier.Create("main");
        var department = Department.Create(name.Value, identifier.Value, null);
        
        return await ExecuteInDb<Guid>(async (dbContext) =>
        {
            await dbContext.Departments.AddAsync(department.Value, CancellationToken.None);
            await dbContext.SaveChangesAsync(CancellationToken.None);
            return department.Value.Id;
        });
    }
    
    protected async Task<Department> CreateDepartmentWithLocations()
    {
        var timezone = Timezone.Create("Europe/Moscow").Value;

        var location1 = Location.Create(
            LocationName.Create("Москва-Сити офис").Value,
            Address.Create("Россия", "Москва", "Пресненская наб.", "10", 20).Value,
            timezone).Value;

        var location2 = Location.Create(
            LocationName.Create("Санкт-Петербург офис").Value,
            Address.Create("Россия", "Санкт-Петербург", "Невский пр.", "25", 5).Value,
            timezone).Value;

        var location3 = Location.Create(
            LocationName.Create("Казань офис").Value,
            Address.Create("Россия", "Казань", "Кремлёвская", "7", 3).Value,
            timezone).Value;

        var deptName = DepartmentName.Create("Тестовый департамент").Value;
        var identifier = Identifier.Create("main").Value;

        var department = Department.Create(deptName, identifier, null).Value;

        department.AddDepartmentLocations(new List<DepartmentLocation>
        {
            new DepartmentLocation(department.Id, location1.Id),
            new DepartmentLocation(department.Id, location2.Id),
            new DepartmentLocation(department.Id, location3.Id)
        });

        return await ExecuteInDb<Department>(async (dbContext) =>
        {
            await dbContext.Locations.AddRangeAsync(location1, location2, location3);
            await dbContext.Departments.AddAsync(department, CancellationToken.None);

            await dbContext.SaveChangesAsync(CancellationToken.None);
            return department;
        });
    }
    
    protected async Task<List<Guid>> CreateManyLocations()
    {
        var timezone = Timezone.Create("Europe/Moscow").Value;

        var locations = new List<Location>
        {
            Location.Create(
                LocationName.Create("Москва новый").Value,
                Address.Create("Россия", "Москва", "Новый адрес", "10", 20).Value,
                timezone).Value,

            Location.Create(
                LocationName.Create("Санкт-Петербург новый").Value,
                Address.Create("Россия", "Санкт-Петербург", "Новый адрес", "25", 5).Value,
                timezone).Value,

            Location.Create(
                LocationName.Create("Казань новый").Value,
                Address.Create("Россия", "Казань", "Новый адрес", "7", 3).Value,
                timezone).Value
        };

        return await ExecuteInDb<List<Guid>>(async (dbContext) =>
        {
            await dbContext.Locations.AddRangeAsync(locations, CancellationToken.None);
            await dbContext.SaveChangesAsync(CancellationToken.None);
            return locations.Select(l => l.Id).ToList();
        });
    }

    protected async Task<List<Guid>> CreateDepartmentsHierarchy()
    {
        // Главный офис (main)
        var main = Department.Create(
            DepartmentName.Create("Главный офис").Value,
            Identifier.Create("main").Value,
            null).Value;

        // Московский отдел (child of main)
        var moscow = Department.Create(
            DepartmentName.Create("Московский отдел").Value,
            Identifier.Create("moscow").Value,
            main).Value;
        moscow.SetParent(main);

        // Маркетинг (child of moscow)
        var marketing = Department.Create(
            DepartmentName.Create("Маркетинговый отдел").Value,
            Identifier.Create("marketing").Value,
            moscow).Value;
        marketing.SetParent(moscow);

        // Инженеры (child of moscow)
        var engineers = Department.Create(
            DepartmentName.Create("Инженерный отдел").Value,
            Identifier.Create("engineers").Value,
            moscow).Value;
        engineers.SetParent(moscow);

        // Отдел продаж (root)
        var sales = Department.Create(
            DepartmentName.Create("Отдел продаж").Value,
            Identifier.Create("sales").Value,
            null).Value;

        // Магазины (child of sales)
        var shops = Department.Create(
            DepartmentName.Create("Магазины").Value,
            Identifier.Create("shops").Value,
            sales).Value;
        shops.SetParent(sales);

        // Колл-центр (child of sales)
        var callCenter = Department.Create(
            DepartmentName.Create("Колл центр").Value,
            Identifier.Create("call-center").Value,
            sales).Value;
        callCenter.SetParent(sales);

        return await ExecuteInDb<List<Guid>>(async (dbContext) =>
        {
            await dbContext.Departments.AddRangeAsync(
                main, moscow, marketing, engineers,
                sales, shops, callCenter);

            await dbContext.SaveChangesAsync(CancellationToken.None);

            return new List<Guid>
            {
                main.Id, moscow.Id, marketing.Id, engineers.Id,
                sales.Id, shops.Id, callCenter.Id
            };
        });
    }
}