using DirectoryService.Contracts.Dtos;

namespace DirectoryService.Contracts.Departments.GetChildren;

public record GetDepartmentChildrenResponse
{
    public required List<DepartmentWithChildrenDto> Departments { get; init; }
}