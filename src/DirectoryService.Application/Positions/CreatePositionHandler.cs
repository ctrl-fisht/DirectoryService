using CSharpFunctionalExtensions;
using DirectoryService.Application.Extensions;
using DirectoryService.Application.Repositories;
using DirectoryService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace DirectoryService.Application.Positions;

public class CreatePositionHandler
{
    private readonly IPositionsRepository _positionsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly CreatePositionValidator _validator;
    private readonly ILogger<CreatePositionHandler> _logger;
    
    public CreatePositionHandler(
        IPositionsRepository positionsRepository,
        IDepartmentsRepository departmentsRepository,
        CreatePositionValidator validator,
        ILogger<CreatePositionHandler> logger)
    {
        _positionsRepository = positionsRepository;
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> HandleAsync(
        CreatePositionCommand command,
        CancellationToken cancellationToken)
    {
        // валидация инпутов
        var validationResult = await _validator.ValidateAsync(command);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToAppErrors();
        
        // Проверка: name не должен совпадать
        var isNameExist = await _positionsRepository.ExistByNameAsync(command.Request.Name, cancellationToken);
        if (isNameExist)
            return AppErrors.General.AlreadyExists("name").ToErrors();
        
        // Проверка: переданные departmentIds существуют и активны
        var isDepartmentsExists = await _departmentsRepository
            .AllExistsAsync(command.Request.DepartmentIds, cancellationToken);
        
        if (!isDepartmentsExists)
            return AppErrors.General.GivenIdsNotExists("departmentIds").ToErrors();

        
        // создание доменных сущностей
        var positionCreateResult = Position.Create(command.Request.Name, command.Request.Description);
        if (positionCreateResult.IsFailure)
            return positionCreateResult.Error.ToErrors();

        var position = positionCreateResult.Value;
        
        var departmentPositions = command.Request.DepartmentIds
            .Select(departmentId => new DepartmentPosition(departmentId, position.Id))
            .ToList();
        
        // добавление DepartmentPositions (связи)
        var addPositionsResult = position.AddDepartmentPositions(departmentPositions);
        if (addPositionsResult.IsFailure)
           return addPositionsResult.Error.ToErrors();
        
        // добавление в БД
        var createPositionResult = await _positionsRepository.CreateAsync(position, cancellationToken);
        if (createPositionResult.IsFailure)
            return createPositionResult.Error.ToErrors();

        return createPositionResult.Value;
    }
}