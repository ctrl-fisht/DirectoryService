using Microsoft.AspNetCore.Http;
using Shared.Kernel;

namespace Shared.Framework.Results;

public class SuccessResult<TValue> : IResult
{
    private readonly TValue _value;

    public SuccessResult(TValue value)
    {
        _value = value;
    }
    
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        var envelope = Envelope.Ok(_value);
        httpContext.Response.StatusCode = 200;
        return httpContext.Response.WriteAsJsonAsync(envelope);
    }
}