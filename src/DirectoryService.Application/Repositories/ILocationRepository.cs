using DirectoryService.Domain.Entities;

namespace DirectoryService.Application.Repositories;

public interface ILocationRepository
{
    Task<Guid> CreateAsync(Location location, CancellationToken cancellationToken);
}