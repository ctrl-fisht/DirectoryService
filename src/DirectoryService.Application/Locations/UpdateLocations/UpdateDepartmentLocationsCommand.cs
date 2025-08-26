using DirectoryService.Contracts.Departments.UpdateLocations;

namespace DirectoryService.Application.Locations.UpdateLocations;

public record UpdateDepartmentLocationsCommand
{
    public required Guid DepartmentId { get; init; }
    public required UpdateDepartmentLocationsRequest Request { get; init; }
}