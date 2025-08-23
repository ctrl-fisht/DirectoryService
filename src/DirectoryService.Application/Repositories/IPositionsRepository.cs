using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using Shared.Errors;

namespace DirectoryService.Application.Repositories;

public interface IPositionsRepository
{
    Task<Result<Guid, Error>> CreateAsync(Position position, CancellationToken cancellationToken);
    Task<bool> ExistByNameAsync(string name, CancellationToken cancellationToken);
}