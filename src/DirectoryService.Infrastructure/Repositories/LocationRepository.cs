using DirectoryService.Application.Repositories;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.ValueObjects;
using DirectoryService.Infrastructure.EfCore;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _dbContext;

    public LocationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Guid> CreateAsync(Location location, CancellationToken cancellationToken)
    {
        await _dbContext.Locations.AddAsync(location, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return location.Id;
    }

    public async Task<bool> ExistsByNameAsync(LocationName locationName, CancellationToken cancellationToken)
    {
        return await _dbContext.Locations.AnyAsync(l => l.LocationName == locationName, cancellationToken);
    }

    public async Task<bool> ExistsByAddressAsync(Address address, CancellationToken cancellationToken)
    {
        return await _dbContext.Locations.AnyAsync(l => l.Address == address, cancellationToken);
    }
}