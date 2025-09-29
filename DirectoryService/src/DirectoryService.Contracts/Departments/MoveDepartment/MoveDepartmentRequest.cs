namespace DirectoryService.Contracts.Departments.MoveDepartment;

public record MoveDepartmentRequest
{
    public Guid? ParentId { get; init; }
}