namespace DirectoryService.Domain.Entities;

public class DepartmentLocation
{
    public Guid DepartmentId { get; set; }
    public Guid LocationId { get; set; }
    public Department  Department { get; set; }
    public Location  Location { get; set; }
    
    // efcore
    private DepartmentLocation() {}
    
    private DepartmentLocation(Guid departmentId, Guid locationId, Department department, Location  location)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
        Department = department;
        Location = location;
    }

    public static DepartmentLocation Create(Department department, Location location)
    {
        // может какая-то валидация
        
        return new DepartmentLocation(department.Id, location.Id, department, location);
    }
}