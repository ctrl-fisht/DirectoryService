using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared.Kernel.Errors;

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

    public static Identifier CreateDeleted(Identifier identifier)
    {
        return new Identifier(Constants.SoftDeletedLabel + identifier.Value);
    }

    public static Identifier CreateFromDb(string identifier)
    {
        return new Identifier(identifier);
    }
}