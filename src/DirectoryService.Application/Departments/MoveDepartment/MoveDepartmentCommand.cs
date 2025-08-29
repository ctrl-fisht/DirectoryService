using DirectoryService.Contracts.Departments.UpdateLocations;

namespace DirectoryService.Application.Departments.UpdateLocations;

public record MoveDepartmentCommand
{
    public required Guid DepartmentId { get; init; }
    public required UpdateDepartmentLocationsRequest Request { get; init; }
}