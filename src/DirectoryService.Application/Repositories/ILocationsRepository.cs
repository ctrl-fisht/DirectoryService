using CSharpFunctionalExtensions;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.ValueObjects;
using Shared.Errors;

namespace DirectoryService.Application.Repositories;

public interface ILocationsRepository
{
    Task<Result<Guid, Error>> CreateAsync(Location location, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(LocationName locationName, CancellationToken cancellationToken);
    Task<bool> ExistsByAddressAsync(Address address, CancellationToken cancellationToken);
    Task<UnitResult<Error>> DeactivateExclusivesToDepartmentAsync(
        Guid departmentId,
        List<Guid> locationIds,
        CancellationToken cancellationToken);
    Task<bool> AllExistsAsync(List<Guid> locations,CancellationToken cancellationToken);
}