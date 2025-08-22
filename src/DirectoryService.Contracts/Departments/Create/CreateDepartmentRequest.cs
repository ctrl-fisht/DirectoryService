namespace DirectoryService.Contracts.Departments.Create;

public record CreateDepartmentRequest
{
    public required string Name { get; set; }
    public required string Identifier { get; set; }
    public Guid? ParentId { get; set; }
    public required List<Guid> Locations { get; set; }
}