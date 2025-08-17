using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared.Errors;

namespace DirectoryService.Domain.ValueObjects;

public record DepartmentName
{  
    public string Value { get;}
    private DepartmentName(string value)
    {
        Value = value;
    }

    public static Result<DepartmentName, Error> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Validation.CannotBeEmpty(nameof(name));

        if (name.Length < Constants.DepartmentConstants.NameMinLength 
            || name.Length > Constants.DepartmentConstants.NameMaxLength)
        {
            return Errors.Validation.LengthNotInRange(
                nameof(name),
                Constants.DepartmentConstants.NameMinLength,
                Constants.DepartmentConstants.NameMaxLength);
        }

        if (!Regex.IsMatch(name, @"^[А-Яа-яЁё -]+$"))
        {
            return Errors.Validation.BadFormat(nameof(name), "Cyrillic, spaces, hyphen");
        }
        
        return new DepartmentName(name);
    }
}