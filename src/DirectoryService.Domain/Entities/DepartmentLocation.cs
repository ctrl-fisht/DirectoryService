namespace DirectoryService.Domain.Entities;

public class DepartmentLocation
{
    public Guid DepartmentId { get; set; }
    public Guid LocationId { get; set; }
    public Department?  Department { get; set; }
    public Location?  Location { get; set; }
    
    // efcore
    private DepartmentLocation() {}
    
    public DepartmentLocation(Department department, Location  location)
    {
        Department = department;
        Location = location;
    }

    public DepartmentLocation(Guid departmentId, Guid locationId)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
    }
    
}