using CSharpFunctionalExtensions;
using Shared.Errors;

namespace DirectoryService.Domain.ValueObjects;

public record Timezone
{
    public string Value { get; }
    
    private Timezone(string value)
    {
        Value = value;
    }

    public static Result<Timezone, Error> Create(string timezone)
    {
        if (string.IsNullOrWhiteSpace(timezone))
            return AppErrors.Validation.CannotBeEmpty(nameof(timezone));

        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timezone);
        }
        catch (TimeZoneNotFoundException)
        {
            return AppErrors.Validation.BadFormat(nameof(timezone), "IANA timezone code");
        }
        catch (InvalidTimeZoneException)
        {
            return AppErrors.Validation.BadFormat(nameof(timezone), "IANA timezone code");
        }

        return new Timezone(timezone);
    }
}