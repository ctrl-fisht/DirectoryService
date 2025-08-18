using DirectoryService.Application.Repositories;
using DirectoryService.Domain.Entities;
using DirectoryService.Infrastructure.EfCore;

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
        await _dbContext.AddAsync(location, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return location.Id;
    }
}