using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Shared.Kernel.Errors;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Shared.Framework.Results;

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

    public EndpointResult(TValue value)
    {
        _value = new SuccessResult<TValue>(value);
    }
    
    public Task ExecuteAsync(HttpContext httpContext)
    {
        return _value.ExecuteAsync(httpContext);
    }

    public static implicit operator EndpointResult<TValue>(Result<TValue, Error> result) => new(result);
    public static implicit operator EndpointResult<TValue>(Result<TValue, Errors> result) => new(result);
    public static implicit operator EndpointResult<TValue>(TValue value) => new(value);
}