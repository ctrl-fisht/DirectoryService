namespace DirectoryService.Contracts.Dtos;

public record DepartmentWithPositionsDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Identifier { get; init; }
    public string Path { get; init; }
    public int Depth { get; init; }
    public DateTime CreatedAt { get; init; }
    public long PositionsCount { get; init; }
}