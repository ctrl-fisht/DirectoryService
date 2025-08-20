using DirectoryService.Domain.Entities;
using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.Application.Repositories;

public interface ILocationRepository
{
    Task<Guid> CreateAsync(Location location, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(LocationName locationName, CancellationToken cancellationToken);
    Task<bool> ExistsByAddressAsync(Address address, CancellationToken cancellationToken);
}