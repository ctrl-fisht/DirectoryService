using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared.Errors;

namespace DirectoryService.Domain.ValueObjects;

public record Address
{
    public string Country { get;  }
    public string City { get; }
    public string Street { get; }
    public string Building { get; }

    private Address(string country, string city, string street, string building)
    {
        Country = country;
        City = city;
        Street = street;
        Building = building;
    }

    public static Result<Address, Error> Create(string country, string city, string street, string building)
    {
        if (string.IsNullOrWhiteSpace(country))
            return Errors.Validation.CannotBeEmpty(nameof(country));

        if (string.IsNullOrWhiteSpace(city))
            return Errors.Validation.CannotBeEmpty(nameof(city));

        if (string.IsNullOrWhiteSpace(street))
            return Errors.Validation.CannotBeEmpty(nameof(street));

        if (string.IsNullOrWhiteSpace(building))
            return Errors.Validation.CannotBeEmpty(nameof(building));

        // Можно добавить регулярки, если нужны ограничения на символы
        var nameRegex = @"^(?=.*[A-Za-zА-Яа-яЁё0-9])[A-Za-zА-Яа-яЁё0-9\s.\-]+$";

        if (!Regex.IsMatch(country, nameRegex))
            return Errors.Validation.BadFormat(nameof(country), "Cyrillic, Latin, digits, spaces, hyphen, dots");

        if (!Regex.IsMatch(city, nameRegex))
            return Errors.Validation.BadFormat(nameof(city), "Cyrillic, Latin, digits, spaces, hyphen, dots");

        if (!Regex.IsMatch(street, nameRegex))
            return Errors.Validation.BadFormat(nameof(street), "Cyrillic, Latin, digits, spaces, hyphen, dots");

        if (!Regex.IsMatch(building, @"^[A-Za-zА-Яа-яЁё0-9\s\-]+$"))
            return Errors.Validation.BadFormat(nameof(building), "Cyrillic, Latin, digits, spaces, hyphen");

        return Result.Success<Address, Error>(new Address(country, city, street, building));
    }
}