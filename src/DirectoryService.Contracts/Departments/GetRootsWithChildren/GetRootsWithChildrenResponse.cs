using DirectoryService.Contracts.Dtos;

namespace DirectoryService.Contracts.Departments.GetRootsWithChildren;

public class GetRootsWithChildrenResponse
{
    public required List<DepartmentWithChildrenDto> Roots { get; init; } = new();
}