using CSharpFunctionalExtensions;
using DirectoryService.Application.Extensions;
using DirectoryService.Application.Repositories;
using DirectoryService.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace DirectoryService.Application.Departments.UpdateLocations;

public class MoveDepartmentHandler
{
    private readonly IDepartmentsRepository _departmentRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly IValidator<MoveDepartmentCommand> _validator;
    private readonly ILogger<MoveDepartmentHandler> _logger;
    
    public MoveDepartmentHandler(
        IDepartmentsRepository departmentRepository,
        ILocationsRepository locationsRepository,
        IValidator<MoveDepartmentCommand> validator,
        ILogger<MoveDepartmentHandler> logger)
    {
        _departmentRepository = departmentRepository;
        _locationsRepository = locationsRepository;
        _validator = validator;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Errors>> HandleAsync(
        MoveDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var locationIds = command.Request.LocationIds.ToList(); 
        
        // валидация команды 
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.Errors.ToAppErrors();
        
        // существует ли подразделение
        var getDepartmentResult = await _departmentRepository.GetAsync(command.DepartmentId, cancellationToken);
        if (getDepartmentResult.IsFailure)
            return getDepartmentResult.Error.ToErrors();
        var department = getDepartmentResult.Value;
        
        
        // получаем locations (заодно проверка на существование)
        var isLocationsExist = await _locationsRepository.AllExistsAsync(locationIds, cancellationToken);
        if (!isLocationsExist)
            return AppErrors.General.GivenIdsNotExists("locations").ToErrors();
        
        // создаем доменные DepartmentLocations
        var departmentLocations = command.Request.LocationIds
            .Select(locationId => new DepartmentLocation(department.Id, locationId))
            .ToList();
        
        department.UpdateDepartmentLocations(departmentLocations);

        var saveChangesResult = await _departmentRepository.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error.ToErrors();

        return department.Id;
    }
}