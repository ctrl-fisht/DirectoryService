namespace DirectoryService.Contracts.Positions.Create;

public class CreatePositionRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required List<Guid> DepartmentIds { get; set; }
}