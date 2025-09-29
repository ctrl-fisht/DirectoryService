using CSharpFunctionalExtensions;
using Shared.Core.Validation;
using DirectoryService.Application.Repositories;
using DirectoryService.Domain;
using DirectoryService.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Core.Caching;
using Shared.Kernel.Errors;

namespace DirectoryService.Application.Departments.UpdateLocations;

public class UpdateDepartmentLocationsHandler
{
    private readonly IDepartmentsRepository _departmentRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<UpdateDepartmentLocationsCommand> _validator;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UpdateDepartmentLocationsHandler> _logger;
    
    public UpdateDepartmentLocationsHandler(
        IDepartmentsRepository departmentRepository,
        ILocationsRepository locationsRepository,
        IValidator<UpdateDepartmentLocationsCommand> validator,
        ICacheService cacheService,
        ILogger<UpdateDepartmentLocationsHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
        _cacheService = cacheService;
    }
    
    public async Task<Result<Guid, Errors>> HandleAsync(
        UpdateDepartmentLocationsCommand locationsCommand,
        CancellationToken cancellationToken)
    {
        var locationIds = locationsCommand.Request.LocationIds.ToList(); 
        
        // валидация команды 
        var validationResult = await _validator.ValidateAsync(locationsCommand, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.Errors.ToAppErrors();
        
        // существует ли подразделение
        var getDepartmentResult = await _departmentRepository.GetAsync(locationsCommand.DepartmentId, cancellationToken);
        if (getDepartmentResult.IsFailure)
            return getDepartmentResult.Error.ToErrors();
        var department = getDepartmentResult.Value;
        
        
        // получаем locations (заодно проверка на существование)
        var isLocationsExist = await _locationsRepository.AllExistsAsync(locationIds, cancellationToken);
        if (!isLocationsExist)
            return AppErrors.General.GivenIdsNotExists("locations").ToErrors();
        
        // создаем доменные DepartmentLocations
        var departmentLocations = locationsCommand.Request.LocationIds
            .Select(locationId => new DepartmentLocation(department.Id, locationId))
            .ToList();
        
        department.UpdateDepartmentLocations(departmentLocations);

        var saveChangesResult = await _departmentRepository.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error.ToErrors();
        
        // инвалидируем кэш
        await _cacheService.RemoveByPrefixAsync(Constants.DepartmentConstants.CachePrefix, cancellationToken);
        
        _logger.LogInformation("Department {@DepartmentId} locations has been updated new locations: {@LocationIds}",
            department.Id,
            locationsCommand.Request.LocationIds);
        
        return department.Id;
    }
}