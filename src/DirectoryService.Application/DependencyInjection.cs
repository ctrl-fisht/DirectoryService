using DirectoryService.Application.Departments.Create;
using DirectoryService.Application.Departments.MoveDepartment;
using DirectoryService.Application.Departments.UpdateLocations;
using DirectoryService.Application.Locations.Create;
using DirectoryService.Application.Positions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        services.AddValidatorsFromAssembly(assembly);
        
        services.AddScoped<CreateLocationHandler>();
        
        services.AddScoped<CreateDepartmentHandler>();
        services.AddScoped<UpdateDepartmentLocationsHandler>();
        services.AddScoped<MoveDepartmentHandler>();
        
        services.AddScoped<CreatePositionHandler>();
        return services;
    }    
}