using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using Shared.Errors;

namespace DirectoryService.Application.Repositories;

public interface IDepartmentRepository
{
    Task<Result<Guid, Error>> AddAsync(Department department, CancellationToken cancellationToken);
    Task<Result<Department, Error>> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> IsIdentifierExistAsync(string identifier, CancellationToken cancellationToken);
}