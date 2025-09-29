using CSharpFunctionalExtensions;
using DirectoryService.Application.Repositories;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.ValueObjects;
using DirectoryService.Infrastructure.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Kernel.Errors;

namespace DirectoryService.Infrastructure.Repositories;

public class LocationsesRepository : ILocationsRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<LocationsesRepository> _logger;
    public LocationsesRepository(AppDbContext dbContext, ILogger<LocationsesRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<Result<Guid, Error>> CreateAsync(Location location, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Locations.AddAsync(location, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return location.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating location");
            return AppErrors.Database.ErrorWhileAdding("location");
        }
    }

    public async Task<bool> ExistsByNameAsync(LocationName locationName, CancellationToken cancellationToken)
    {
        return await _dbContext.Locations.AnyAsync(l => l.LocationName == locationName, cancellationToken);
    }

    public async Task<bool> ExistsByAddressAsync(Address address, CancellationToken cancellationToken)
    {
        return await _dbContext.Locations.AnyAsync(l => l.Address == address, cancellationToken);
    }

    public async Task<UnitResult<Error>> DeactivateExclusivesToDepartmentAsync(
        Guid departmentId,
        List<Guid> locationIds,
        CancellationToken cancellationToken)
    { 
        var utcNow = DateTime.UtcNow;

        var query = _dbContext.Locations
            .Where(l => locationIds.Contains(l.Id))
            .Where(l => !l.DepartmentLocations
                .Any(dl => dl.DepartmentId != departmentId && dl.Department!.IsActive));
        try
        {
            var updated = await query.ExecuteUpdateAsync(setters => setters
                    .SetProperty(l => l.IsActive, false)
                    .SetProperty(l => l.DeletedAt, utcNow),
                cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deactivating locations");
            return AppErrors.Database.ErrorWhileUpdating("locations");
        }
        
    }

    public async Task<bool> AllExistsAsync(List<Guid> locations, CancellationToken cancellationToken)
    {
        var count = await _dbContext.Locations
            .Where(l => locations.Contains(l.Id))
            .Where(l => l.IsActive)
            .CountAsync(cancellationToken);
        
        return count == locations.Count;
    }
}