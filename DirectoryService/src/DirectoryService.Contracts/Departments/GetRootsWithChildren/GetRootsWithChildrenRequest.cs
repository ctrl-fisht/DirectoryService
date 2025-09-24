namespace DirectoryService.Contracts.Departments.GetRootsWithChildren;

public record GetRootsWithChildrenRequest
{
    public int? Page { get; init; }
    public int? PageSize { get; init; }
    public int? Prefetch { get; init; }
}