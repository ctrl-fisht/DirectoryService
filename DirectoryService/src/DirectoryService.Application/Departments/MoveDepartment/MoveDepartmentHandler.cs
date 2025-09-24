using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Extensions;
using DirectoryService.Application.Repositories;
using DirectoryService.Domain;
using DirectoryService.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Caching;
using Shared.Errors;

namespace DirectoryService.Application.Departments.MoveDepartment;

public class MoveDepartmentHandler
{
    private readonly IValidator<MoveDepartmentCommand> _validator;
    private readonly ITransactionManager _transactionManager;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<MoveDepartmentHandler> _logger;
    
    public MoveDepartmentHandler(
        IValidator<MoveDepartmentCommand> validator,
        ITransactionManager transactionManager, 
        IDepartmentsRepository departmentsRepository, 
        ICacheService cacheService,
        ILogger<MoveDepartmentHandler> logger)
    {
        _validator = validator;
        _transactionManager = transactionManager;
        _departmentsRepository = departmentsRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> HandleAsync(
        MoveDepartmentCommand command,
        CancellationToken cancellationToken)
    {
        // валидация инпутов
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.Errors.ToAppErrors();

        // открываем транзакцию
        var (_, isFailure, transaction, error) = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (isFailure)
            return error.ToErrors();
        
        // проверка существования ПОДРАЗДЕЛЕНИЯ КОТОРОЕ ПЕРЕНОСИМ + пессимистическая блокировка
        var getDepartmentResult =
            await _departmentsRepository.GetByIdWithLockAsync(command.DepartmentId, cancellationToken);
        if (getDepartmentResult.IsFailure)
        {
            transaction.Rollback(cancellationToken);
            return getDepartmentResult.Error.ToErrors();
        }
        var department = getDepartmentResult.Value;

        // если переданный parentId = null , то хотим перенести в корень
        // иначе получаем родителя + пессимистическая блокировка
        Department? parentDepartment = null;
        if (command.Request.ParentId != null)
        {
            var getParentResult = await _departmentsRepository
                .GetByIdWithLockAsync(command.Request.ParentId.Value, cancellationToken);
            if (getParentResult.IsFailure)
            {
                transaction.Rollback(cancellationToken);
                return getParentResult.Error.ToErrors();
            }
            parentDepartment = getParentResult.Value;
        }
        
        // Проверка, не является ли вдруг новый родитель потомком данного department 
        if (command.Request.ParentId != null)
        {
            var isDescendant = await _departmentsRepository.IsDescendantAsync(
                department.Path, 
                command.Request.ParentId.Value, 
                cancellationToken);
            if (isDescendant)
            {
                transaction.Rollback(cancellationToken);
                return AppErrors.Hierarchy.CannotAddChildAsAncestor().ToErrors();
            }
        }
        
        // блокируем всех потомков  
        var lockDescendantsResult =
            await _departmentsRepository.LockDescendantsAsync(department, cancellationToken);
        if (lockDescendantsResult.IsFailure)
        {
            transaction.Rollback(cancellationToken);
            return lockDescendantsResult.Error.ToErrors();
        }

        // old path нужен для запроса на обновление путей в БД
        var oldPath = department.Path;

        // Меняем родителя, доменная модель посчитает новый path и depth для департмента
        department.SetParent(parentDepartment);
        
        // Обновляем также path и depth у всех потомков этого department с помощью ltree
        var updateResult = await _departmentsRepository.MoveDepartmentAsync(department, oldPath, cancellationToken);
        if (updateResult.IsFailure)
        {
            transaction.Rollback(cancellationToken);
            return updateResult.Error.ToErrors();
        }

        var commitResult = transaction.Commit(cancellationToken);
        if (commitResult.IsFailure)
            return commitResult.Error.ToErrors();

        // инвалидируем кэш
        await _cacheService.RemoveByPrefixAsync(Constants.DepartmentConstants.CachePrefix, cancellationToken);
        
        _logger.LogInformation(
            "Parent successfully updated Department id='{@Id}' new parent_id={@ParentId}",
            department.Id,
            department.ParentId);
        
        return department.Id;
    }
}