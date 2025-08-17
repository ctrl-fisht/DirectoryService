using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared.Errors;

namespace DirectoryService.Domain.ValueObjects;

public record LocationName
{
    public string Value { get; }
    
    private LocationName(string value)
    {
        Value = value;
    }

    public static Result<LocationName, Error> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.Validation.CannotBeEmpty(nameof(name));

        if (name.Length < Constants.LocationConstants.NameMinLength 
            || name.Length > Constants.LocationConstants.NameMaxLength)
        {
            return Errors.Validation.LengthNotInRange(
                nameof(name),
                Constants.LocationConstants.NameMinLength,
                Constants.LocationConstants.NameMaxLength);
        }

        if (!Regex.IsMatch(name, @"^(?=.*[A-Za-zА-Яа-яЁё0-9])[A-Za-zА-Яа-яЁё0-9\s.\-]+$"))
        {
            return Errors.Validation.BadFormat(nameof(name), "Cyrillic,Latin, spaces, hyphen, dots");
        }
        
        return new LocationName(name);
    }
}