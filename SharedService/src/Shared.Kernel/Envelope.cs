using System.Text.Json.Serialization;

namespace Shared.Kernel;

public record Envelope
{
    public object? Result { get;}
    public Errors.Errors? Errors { get;}

    public bool IsSuccess() => Errors == null;
    
    public DateTime TimeGenerated { get; }

    [JsonConstructor]
    private Envelope(object? result, Errors.Errors? errors)
    {
        Result = result;
        Errors = errors;
        TimeGenerated = DateTime.Now;
    }

    public static Envelope Ok(object? result = null) => new Envelope(result, null);
    public static Envelope Error(Errors.Errors errors) => new Envelope(null, errors);
}