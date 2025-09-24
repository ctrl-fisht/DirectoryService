namespace DirectoryService.Application.Departments.GetDepartmentChildren;

public record GetDepartmentChildrenQuery
{
    public required Guid ParentId { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
}