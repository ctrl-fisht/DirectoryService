using DirectoryService.Contracts.Departments.Create;

namespace DirectoryService.Application.Departments.Create;

public record CreateDepartmentCommand
{
    public required CreateDepartmentRequest Request { get; set; }
}