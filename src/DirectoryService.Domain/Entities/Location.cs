using CSharpFunctionalExtensions;
using DirectoryService.Domain.ValueObjects;
using Shared.Errors;

namespace DirectoryService.Domain.Entities;

public class Location
{
    public Guid Id { get; private set; }
    public LocationName LocationName { get; private set; }
    public Address Address { get; private set; }
    public Timezone Timezone { get; private set; }
    
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<DepartmentLocation> _departmentLocations = [];
    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations.AsReadOnly();

    
    // efcore
    private Location() {}
    
    public UnitResult<Error> AddDepartmentLocation(DepartmentLocation departmentLocation)
    {
        _departmentLocations.Add(departmentLocation);
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> RemoveDepartmentLocation(DepartmentLocation departmentLocation)
    {
        _departmentLocations.Remove(departmentLocation);
        return UnitResult.Success<Error>();
    }
    
    private Location(Guid id , LocationName locationName, Address address, Timezone timezone)
    {
        Id = id;
        LocationName = locationName;
        Address = address;
        Timezone = timezone;
        
        IsActive = true;
        var utcNow = DateTime.UtcNow;
        CreatedAt = utcNow;
        UpdatedAt = utcNow;
    }

    public static Result<Location, Error> Create(LocationName locationName, Address address, Timezone timezone)
    {
        var id = Guid.NewGuid();
        return new Location(id, locationName, address, timezone);
    }
}