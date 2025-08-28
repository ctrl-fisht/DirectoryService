using DirectoryService.Contracts.Departments.UpdateParent;

namespace DirectoryService.Application.Departments.UpdateParent;

public record UpdateDepartmentParentCommand
{
    public required Guid DepartmentId { get; init; }
    public required UpdateDepartmentParentRequest Request { get; init; }
}