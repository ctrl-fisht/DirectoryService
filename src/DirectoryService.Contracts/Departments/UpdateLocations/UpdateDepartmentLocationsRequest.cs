namespace DirectoryService.Contracts.Departments.UpdateLocations;

public record UpdateDepartmentLocationsRequest
{
    public required IEnumerable<Guid> LocationIds { get; init; }
}