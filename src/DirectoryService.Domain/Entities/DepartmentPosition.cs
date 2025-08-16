namespace DirectoryService.Domain.Entities;

public class DepartmentPosition
{
    public Guid DepartmentId { get; set; }
    public Guid PositionId { get; set; }
    public Department  Department { get; set; }
    public Position Position { get; set; }

    private DepartmentPosition(Guid departmentId, Guid positionId, Department department, Position  position)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
        Department = department;
        Position = position;
    }

    public static DepartmentPosition Create(Department department, Position position)
    {
        // может какая-то валидация
        
        return new DepartmentPosition(department.Id, position.Id, department, position);
    }
}