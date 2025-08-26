namespace DirectoryService.Domain.Entities;

public class DepartmentPosition
{
    public Guid DepartmentId { get; set; }
    public Guid PositionId { get; set; }
    public Department?  Department { get; set; }
    public Position? Position { get; set; }
    
    
    // efcore
    private DepartmentPosition() { }
    
    public DepartmentPosition(Guid departmentId, Guid positionId)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
    }

    public DepartmentPosition(Department department, Position position)
    {
        Department = department;
        Position = position;
        DepartmentId = department.Id;
        PositionId = position.Id;
    }

}