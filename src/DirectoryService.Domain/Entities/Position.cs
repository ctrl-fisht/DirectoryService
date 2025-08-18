using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared.Errors;

namespace DirectoryService.Domain.Entities;

public class Position
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    
    private List<DepartmentPosition> _departmentPositions;
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
    
    private Position(Guid id, string name, string description)
    {
        Id = id;
        Name = name;
        Description = description;
        
        IsActive = true;
        var utcNow = DateTime.UtcNow;
        CreatedAt = utcNow;
        UpdatedAt = utcNow;
    }

    public static Result<Position, Error> Create(string name, string description)
    {
        var id = Guid.NewGuid();

        if (string.IsNullOrWhiteSpace(name))
            return AppErrors.Validation.CannotBeEmpty(nameof(name));
        
        if (name.Length > Constants.PositionConstants.NameMaxLength 
            || name.Length < Constants.PositionConstants.NameMinLength)
            return AppErrors.Validation.LengthNotInRange(
                nameof(name),
                Constants.PositionConstants.NameMinLength,
                Constants.PositionConstants.NameMaxLength);
        
        if (!Regex.IsMatch(name, @"^[A-Za-zА-Яа-яЁё\s.-]+$"))
        {
            return AppErrors.Validation.BadFormat(
                nameof(name), 
                "Cyrillic, Latin, spaces, hyphen, dots");
        }
        
        if (!string.IsNullOrWhiteSpace(description) 
            && description.Length > Constants.PositionConstants.DescriptionMaxLength)
        {
            return AppErrors.Validation.TooLong(nameof(name), Constants.PositionConstants.DescriptionMaxLength);
        }

        return new Position(id, name, description);
    }
}

