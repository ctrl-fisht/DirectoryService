using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.ValueObjects;
using Shared.Errors;

namespace DirectoryService.Application.Repositories;

public interface ILocationRepository
{
    Task<Result<Guid, Error>> CreateAsync(Location location, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(LocationName locationName, CancellationToken cancellationToken);
    Task<bool> ExistsByAddressAsync(Address address, CancellationToken cancellationToken);
    Task<bool> AllExistsAsync(List<Guid> locations,CancellationToken cancellationToken);
}