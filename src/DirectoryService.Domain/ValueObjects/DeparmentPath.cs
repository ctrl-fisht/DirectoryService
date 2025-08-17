using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared.Errors;

namespace DirectoryService.Domain.ValueObjects;

public record DeparmentPath
{
    public string Value { get;}

    private DeparmentPath(string path)
    {
        Value = path;
    }

    public static Result<DeparmentPath, Error> Create(string path)
    {
        if (!Regex.IsMatch(path, @"^(?=.*[A-Za-z])[A-Za-z.-]+$"))
        {
            return Errors.Validation.BadFormat(nameof(path), "Latin letters, dots, hyphen");
        }

        return new DeparmentPath(path);
    }
}