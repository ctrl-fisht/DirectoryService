using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shared.Errors;

namespace DirectoryService.Domain.ValueObjects;

public record DepartmentPath
{
    public string Value { get;}

    private DepartmentPath(string value)
    {
        Value = value;
    }

    public static Result<DepartmentPath, Error> CalculatePath(DepartmentPath? parentPath, Identifier identifier)
    {
        if (parentPath == null)
        {
            var pathCreateResult = Create(identifier.Value);
            if (!pathCreateResult.IsSuccess)
                return pathCreateResult.Error;
            return pathCreateResult.Value;
        }

        var newPathString = parentPath.Value + "." + identifier.Value;
        var newPathCreateResult = Create(newPathString);
        if (!newPathCreateResult.IsSuccess)
            return newPathCreateResult.Error;
        return newPathCreateResult.Value;
    }
    
    public static Result<DepartmentPath, Error> Create(string path)
    {
        if (!Regex.IsMatch(path, @"^(?=.*[A-Za-z])[A-Za-z.-]+$"))
        {
            return AppErrors.Validation.BadFormat(nameof(path), "Latin letters, dots, hyphen");
        }

        return new DepartmentPath(path.ToLower());
    }

    public static DepartmentPath CreateFromDb(string path)
    {
        return new DepartmentPath(path);
    }
}