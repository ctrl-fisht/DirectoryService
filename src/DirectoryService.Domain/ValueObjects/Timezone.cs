using CSharpFunctionalExtensions;
using Shared.Errors;

namespace DirectoryService.Domain.ValueObjects;

public record Timezone
{
    public string Value { get; }

    // efcore
    private Timezone(){}

    private Timezone(string timezone)
    {
        Value = timezone;
    }

    public static Result<Timezone, Error> Create(string timezone)
    {
        if (string.IsNullOrWhiteSpace(timezone))
            return Errors.Validation.CannotBeEmpty(nameof(timezone));

        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timezone);
        }
        catch (TimeZoneNotFoundException)
        {
            return Errors.Validation.BadFormat(nameof(timezone), "IANA timezone code");
        }
        catch (InvalidTimeZoneException)
        {
            return Errors.Validation.BadFormat(nameof(timezone), "IANA timezone code");
        }

        return new Timezone(timezone);
    }
}