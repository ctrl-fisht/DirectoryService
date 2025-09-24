using CSharpFunctionalExtensions;
using Shared.Core.Validation;
using DirectoryService.Application.Repositories;
using DirectoryService.Domain;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Core.Database;
using Shared.Core.Caching;
using Shared.Kernel.Errors;

namespace DirectoryService.Application.Departments.Deactivate;

public class DeactivateDepartmentHandler
{
    private readonly IValidator<DeactivateDepartmentCommand> _validator;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ILocationsRepository _locationsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DeactivateDepartmentHandler> _logger;
    
    public DeactivateDepartmentHandler(
        IValidator<DeactivateDepartmentCommand> validator,
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        ILocationsRepository locationsRepository,
        ICacheService cacheService,
        ILogger<DeactivateDepartmentHandler> logger)
    {
        _validator = validator;
        _departmentsRepository = departmentsRepository;
        _locationsRepository = locationsRepository;
        _transactionManager = transactionManager;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> HandleAsync(
        DeactivateDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command);
        if (!validationResult.IsValid)
            return validationResult.Errors.ToAppErrors();

        // Проверка: существует ли такое подразделение и оно активно?
        var departmentResult = await _departmentsRepository.GetAsync(command.Id, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error.ToErrors();
        
        // Деактивируем подразделение
        var openTransactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (openTransactionResult.IsFailure)
            return openTransactionResult.Error.ToErrors();
        
        var transaction = openTransactionResult.Value;
        var department = departmentResult.Value;

        var oldPath = department.Path;
        
        var result = department.Deactivate();
        if (result.IsFailure)
            return result.Error.ToErrors();

        var saveChangesResult = await _departmentsRepository.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error.ToErrors();
        
        // Деактивация локаций связанных только с этим подразделением
        var locationIds = department.DepartmentLocations.Select(dl => dl.LocationId).ToList();
        if (locationIds.Any())
        {
            var locationsResult = 
                await _locationsRepository
                    .DeactivateExclusivesToDepartmentAsync(department.Id, locationIds, cancellationToken);
            
            if (locationsResult.IsFailure)
                return locationsResult.Error.ToErrors();
        }
        
        // Обновление Path у дочерних подразделений
        var updatePathsResult = await _departmentsRepository.UpdateChildPaths(department, oldPath.Value, cancellationToken);
        if (updatePathsResult.IsFailure)
            return updatePathsResult.Error.ToErrors();

        var commitResult = transaction.Commit(cancellationToken);
        if (commitResult.IsFailure)
            return commitResult.Error.ToErrors();
        
        // инвалидируем кэш
        await _cacheService.RemoveByPrefixAsync(Constants.DepartmentConstants.CachePrefix, cancellationToken);
        
        _logger.LogInformation(
            "Department {@DepartmentId} has been deactivated, deactivated locations {@LocationsIds}",
            department.Id,
            locationIds);
        
        return department.Id;
    }
}