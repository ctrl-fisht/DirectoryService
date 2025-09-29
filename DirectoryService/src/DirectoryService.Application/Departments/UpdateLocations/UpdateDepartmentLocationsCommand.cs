using DirectoryService.Contracts.Departments.UpdateLocations;

namespace DirectoryService.Application.Departments.UpdateLocations;

public record UpdateDepartmentLocationsCommand
{
    public required Guid DepartmentId { get; init; }
    public required UpdateDepartmentLocationsRequest Request { get; init; }
}