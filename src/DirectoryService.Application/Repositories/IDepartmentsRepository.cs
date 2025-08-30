using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.ValueObjects;
using Shared.Errors;

namespace DirectoryService.Application.Repositories;

public interface IDepartmentsRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken);
    Task<Result<Department, Error>> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> IsIdentifierExistAsync(Identifier identifier, CancellationToken cancellationToken);
    Task<bool> AllExistsAsync(List<Guid> ids, CancellationToken cancellationToken);
    Task<Result<Department, Error>> GetByIdWithLockAsync(Guid id, CancellationToken cancellationToken);
    Task<UnitResult<Error>> LockDescendantsAsync(Department department, CancellationToken cancellationToken);
    Task<bool> IsDescendantAsync (DepartmentPath rootPath, Guid candidateForCheckId, CancellationToken cancellationToken);
    Task<UnitResult<Error>> MoveDepartmentAsync(
        Department departmentUpdated,
        DepartmentPath oldPath, CancellationToken cancellationToken);
    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken); 
}