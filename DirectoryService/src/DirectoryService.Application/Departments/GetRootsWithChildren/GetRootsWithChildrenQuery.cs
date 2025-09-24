namespace DirectoryService.Application.Departments.GetRootsWithChildren;

public class GetRootsWithChildrenQuery
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Prefetch { get; init; }
}