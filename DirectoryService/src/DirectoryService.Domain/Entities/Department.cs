using CSharpFunctionalExtensions;
using DirectoryService.Domain.ValueObjects;
using Shared.Kernel.Errors;

namespace DirectoryService.Domain.Entities;

public class Department
{
    // efcore
    private Department() {}
    private Department(Guid id,
        DepartmentName departmentName,
        Identifier identifier, 
        Department? parent,
        DepartmentPath path,
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
        DeletedAt = null;
    }

    public Guid Id { get; private set; }
    public DepartmentName DepartmentName { get; private set; }
    public Identifier Identifier { get; private set; }
    public Guid? ParentId { get; private set; }
    public Department? Parent { get; private set; }
    public DepartmentPath Path { get; private set; }
    public int Depth { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; } 
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }


    
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
    
    public UnitResult<Error> SetParent(Department? parent)
    {
        if (parent == this)
            return AppErrors.Hierarchy.CannotAddSelfAsAParent();
        
        // пересчитываем path + depth
        var newPathResult = DepartmentPath.CalculatePath(parent?.Path, Identifier);
        if (!newPathResult.IsSuccess)
        {
            return newPathResult.Error;
        }

        int newDepth = 1;
        if (parent != null)
            newDepth = parent.Depth + 1;

        
        ParentId = parent?.Id;
        Depth = newDepth;
        Path = newPathResult.Value;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Deactivate()
    {
        if (!IsActive)
            return AppErrors.Domain.Department.AlreadyDeactivated();
        
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
        
        var oldIdentifier = Identifier;
        Identifier = Identifier.CreateDeleted(Identifier);
        
        var newPath = DepartmentPath.CreateDeleted(oldIdentifier.Value, Path);
        
        Path = newPath;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Activate()
    {
        if (IsActive)
            return AppErrors.Domain.Department.AlreadyActivated();

        var oldDeletedAt = DeletedAt;
        var oldIdentifier = Identifier;
        
        IsActive = true;
        DeletedAt = null;
        
        
        var activeIdentifierResult = Identifier.Create(Identifier.Value.Replace(Constants.SoftDeletedLabel, ""));
        if (activeIdentifierResult.IsFailure)
        {
            IsActive = false;
            DeletedAt = oldDeletedAt;
            return activeIdentifierResult.Error;    
        }
        Identifier = activeIdentifierResult.Value;
        
        var newPathResult = DepartmentPath.Create(Path.Value.Replace(oldIdentifier.Value, Identifier.Value));
        if (newPathResult.IsFailure)
        {
            IsActive = false;
            DeletedAt = oldDeletedAt;
            Identifier = oldIdentifier;
            return newPathResult.Error;
        }
        Path = newPathResult.Value;
        return UnitResult.Success<Error>();
    }
    
    public static Result<Department, Error> Create(
        DepartmentName departmentName,
        Identifier identifier,
        Department? parent)
    {
        var id = Guid.NewGuid();

        var pathCreateResult = DepartmentPath.CalculatePath(parent?.Path, identifier);
        if (!pathCreateResult.IsSuccess)
            return pathCreateResult.Error;

        // depth calculating
        int depth = 1;
        if (parent != null)
            depth = parent.Depth + 1;

        return new Department(id, departmentName, identifier, parent, pathCreateResult.Value, depth);
    }
}