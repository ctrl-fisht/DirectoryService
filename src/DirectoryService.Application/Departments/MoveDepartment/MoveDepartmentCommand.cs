using DirectoryService.Contracts.Departments.MoveDepartment;

namespace DirectoryService.Application.Departments.MoveDepartment;

public record MoveDepartmentCommand
{
    public required Guid DepartmentId { get; init; }
    public required MoveDepartmentRequest Request { get; init; }
}