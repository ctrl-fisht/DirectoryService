using DirectoryService.Contracts.Dtos;

namespace DirectoryService.Contracts.Departments.GetTopPositions;

public record GetTopPositionsResponse
{ 
    public required List<DepartmentWithPositionsDto> DepartmentWithPositions { get; set; }
}