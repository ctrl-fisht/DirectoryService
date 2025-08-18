using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared.Errors;

namespace DirectoryService.Domain.ValueObjects;

public record Identifier
{
    public string Value { get;}

    private Identifier(string value)
    {
        Value = value;
    }

    public static Result<Identifier, Error> Create(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return AppErrors.Validation.CannotBeEmpty(nameof(identifier));
        
        if (identifier.Length < Constants.DepartmentConstants.IdentifierMinLength 
            || identifier.Length > Constants.DepartmentConstants.IdentifierMaxLength)
        {
            return AppErrors.Validation.LengthNotInRange(
                nameof(identifier),
                Constants.DepartmentConstants.IdentifierMinLength,
                Constants.DepartmentConstants.IdentifierMaxLength);
        }
        
        if (!Regex.IsMatch(identifier, @"^[A-Za-z-]+$"))
        {
            return AppErrors.Validation.BadFormat(nameof(identifier), "Latin letters, hyphens");
        }

        return new Identifier(identifier.ToLower());
    }
}