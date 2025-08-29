namespace DirectoryService.Contracts.Departments.UpdateParent;

public record UpdateDepartmentParentRequest
{
    public Guid? ParentId { get; init; }
}