using CSharpFunctionalExtensions;
using DirectoryService.Domain.ValueObjects;
using Shared.Errors;

namespace DirectoryService.Domain.Entities;

public class Department
{
    // efcore
    private Department() {}
    private Department(Guid id,
        DepartmentName departmentName,
        Identifier identifier, 
        Department? parent,
        DeparmentPath path,
        int depth)
    {
        Id = id;
        DepartmentName = departmentName;
        Identifier = identifier;
        Parent = parent;
        Path = path;
        Depth = depth;
        
        IsActive = true;
        var utcNow = DateTime.UtcNow;
        CreatedAt = utcNow;
        UpdatedAt = utcNow;
    }

    public Guid Id { get; private set; }
    public DepartmentName DepartmentName { get; private set; }
    public Identifier Identifier { get; private set; }
    public Guid? ParentId { get; private set; }
    public Department? Parent { get; private set; }
    public DeparmentPath Path { get; private set; }
    public int Depth { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; } 
    public DateTime UpdatedAt { get; private set; }


    
    private readonly List<Department> _children = [];
    private readonly List<DepartmentLocation> _departmentLocations = [];
    private readonly List<DepartmentPosition> _departmentPositions = [];
    
    public IReadOnlyList<Department> Children => _children.AsReadOnly();
    public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations.AsReadOnly();
    public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions.AsReadOnly();
    
    public UnitResult<Error> AddDepartmentPosition(DepartmentPosition departmentPosition)
    {
        _departmentPositions.Add(departmentPosition);
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> RemoveDepartmentPosition(DepartmentPosition departmentPosition)
    {
        _departmentPositions.Remove(departmentPosition);
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> AddDepartmentLocations(List<DepartmentLocation> departmentLocations)
    {
        _departmentLocations.AddRange(departmentLocations);
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> UpdateDepartmentLocations(List<DepartmentLocation> departmentLocations)
    {
        _departmentLocations.Clear();
        _departmentLocations.AddRange(departmentLocations);
        return UnitResult.Success<Error>();
    }
    
    public UnitResult<Error> AddDepartmentLocation(DepartmentLocation departmentLocation)
    {
        _departmentLocations.Add(departmentLocation);
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> RemoveDepartmentLocation(DepartmentLocation departmentLocation)
    {
        _departmentLocations.Remove(departmentLocation);
        return UnitResult.Success<Error>();
    }
    
    public UnitResult<Error> SetParent(Department parent)
    {
        if (parent == this)
            return AppErrors.Hierarchy.CannotAddSelfAsAParent();

        if (parent.Children.FirstOrDefault(c => c.Id == this.Id) == null)
            return AppErrors.Hierarchy.ParentHasNoSuchChild(parent.Id.ToString());
    
        Parent = parent;
        ParentId = parent.Id;
        return UnitResult.Success<Error>();
    }
    
    public UnitResult<Error> AddChild(Department child)
    {
        if (child == this)
            return AppErrors.Hierarchy.CannotAddSelfAsAChild();
        
        if (child.IsAncestorOf(this))
            return AppErrors.Hierarchy.CannotAddAncestor();
        
        // todo: Подумать над проверкой
        // нужно ли проверять переданный Deparment
        // на наличие во всей иерархии
        
        _children.Add(child);
        child.SetParent(this);
        return UnitResult.Success<Error>();
    }
    
    public UnitResult<Error> RemoveChild(Department child)
    {
        var childToRemove = _children.FirstOrDefault(x => x.Id == child.Id);
        if (childToRemove == null)
        {
            return AppErrors.General.NotFound(child.Id.ToString());
        }
        _children.Remove(childToRemove);
        return UnitResult.Success<Error>();
    }
    
    
    private bool IsAncestorOf(Department candidate)
    {
        var current = Parent;
        while (current != null)
        {
            if (current == candidate)
                return true;
            current = current.Parent;
        }
        return false;
    }

    public static Result<Department, Error> Create(
        DepartmentName departmentName,
        Identifier identifier,
        Department? parent)
    {
        var id = Guid.NewGuid();

        // path calculating
        string path = identifier.Value;
        if (parent != null)
            path = parent.Path.Value + "." + identifier.Value;

        var pathCreateResult = DeparmentPath.Create(path);
        if (!pathCreateResult.IsSuccess)
            return pathCreateResult.Error;


        // depth calculating
        int depth = 0;
        if (parent != null)
            depth = parent.Depth + 1;

        return new Department(id, departmentName, identifier, parent, pathCreateResult.Value, depth);
    }
}