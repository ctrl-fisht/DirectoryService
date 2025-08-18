using System.Text.Json.Serialization;

namespace Shared.Errors;

public record Error
{
    public string Code { get; }
    public string Message { get; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ErrorType  Type { get; }
    public string? InvalidField { get; }
    private Error(string code, string message, ErrorType type, string? invalidField = null)
    {
        Code = code;
        Message = message;
        Type = type;
        InvalidField = invalidField;
    }

    public static Error Validation(string code, string message, string?  invalidField = null)
    {
        return new Error(code, message, ErrorType.Validation, invalidField);
    }

    public static Error NotFound(string code, string message)
    {
        return new Error(code, message, ErrorType.NotFound);
    }

    public static Error Conflict(string code, string message)
    {
        return new Error(code, message, ErrorType.Conflict);
    }

    public static Error Failure(string code, string message)
    {
        return new Error(code, message, ErrorType.Failure);
    }
    
    public Errors ToErrors()
    {
        return new Errors(this);
    }
}