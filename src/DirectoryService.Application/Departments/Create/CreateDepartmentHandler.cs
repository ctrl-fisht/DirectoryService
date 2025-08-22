using CSharpFunctionalExtensions;
using DirectoryService.Application.Extensions;
using DirectoryService.Application.Repositories;
using DirectoryService.Contracts.Departments.Create;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace DirectoryService.Application.Departments.Create;

public class CreateDepartmentHandler
{
    private readonly CreateDepartmentValidator _validator;
    private readonly IDepartmentRepository _departmentsRepository;
    private readonly ILocationRepository _locationsRepository;
    private readonly ILogger<CreateDepartmentHandler> _logger;
    
    public CreateDepartmentHandler(
        CreateDepartmentValidator validator,
        IDepartmentRepository departmentsRepository,
        ILocationRepository locationsRepository,
        ILogger<CreateDepartmentHandler> logger)
    {
        _validator = validator;
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _logger = logger;
    }


    public async Task<Result<Guid, Errors>> HandleAsync(
        CreateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        // Валидация инпутов
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToAppErrors();

        // Проверка: локации существуют?
        var isLocationsExists = await _locationsRepository
            .AllExistsAsync(command.Request.Locations, cancellationToken);

        if (!isLocationsExists)
            return AppErrors.General.GivenIdsNotExists("locations").ToErrors();

        // Проверка: идентификатор уникальный?
        var isIdentifierExist = 
            await _departmentsRepository.IsIdentifierExistAsync(command.Request.Identifier, cancellationToken);
        
        if (isIdentifierExist)
            return AppErrors.General.AlreadyExists("identifier").ToErrors();
        
        
        // Создание ValueObject
        var departmentName = DepartmentName.Create(command.Request.Name).Value;
        var identifier = Identifier.Create(command.Request.Identifier).Value;
        
        // Достаём родителя, если он есть, и создаем Department
        var parentId = command.Request.ParentId;

        Result<Department, Error> createDepartmentResult;

        if (parentId == null)
            createDepartmentResult = Department.Create(departmentName, identifier, null);
        else
        {
            var parent = await _departmentsRepository.GetAsync(parentId.Value, cancellationToken);

            if (parent.IsFailure)
                return parent.Error.ToErrors();
            
            createDepartmentResult = Department.Create(departmentName, identifier, parent.Value);
        }

        if (createDepartmentResult.IsFailure)
            return createDepartmentResult.Error.ToErrors();
        
        var department = createDepartmentResult.Value;
        
        // Создаём DepartmentLocations
        var departmentLocations = command.Request.Locations
            .Select(locationId => new DepartmentLocation(
                departmentId: department.Id,
                locationId: locationId))
            .ToList();
        
        // Добавляем DepartmentLocations в доменную модель Department
        var addLocationsResult = department.AddDepartmentLocations(departmentLocations);
        if (addLocationsResult.IsFailure)
            return addLocationsResult.Error.ToErrors();
        
        // Сохраняем Department
        var saveResult = await _departmentsRepository.AddAsync(department, cancellationToken);
        if (saveResult.IsFailure)
            return saveResult.Error.ToErrors();

        return saveResult.Value;
    }
}

