using CSharpFunctionalExtensions;
using Shared.Errors;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace DirectoryService.Presentation.Envelope;

public class EndpointResult<TValue> : IResult
{
    private readonly IResult _value;

    public EndpointResult(Result<TValue, Error> result)
    {
        _value = result.IsSuccess
            ? new SuccessResult<TValue>(result.Value)
            : new FailureResult(result.Error);
    }

    public EndpointResult(Result<TValue, Errors> result)
    {
        _value = result.IsSuccess
            ? new SuccessResult<TValue>(result.Value)
            : new FailureResult(result.Error);
    }
    
    public Task ExecuteAsync(HttpContext httpContext)
    {
        return _value.ExecuteAsync(httpContext);
    }

    public static implicit operator EndpointResult<TValue>(Result<TValue, Error> result) => new(result);
    public static implicit operator EndpointResult<TValue>(Result<TValue, Errors> result) => new(result);
}