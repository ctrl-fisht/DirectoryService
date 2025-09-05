namespace DirectoryService.Contracts.Dtos;

public class DepartmentWithChildrenDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Identifier { get; init; }
    public Guid? ParentId { get; init; }
    public string Path { get; init; }
    public int Depth { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool HasMoreChildren { get; init; }
    public List<DepartmentWithChildrenDto> Children { get; init; } =  new();
}