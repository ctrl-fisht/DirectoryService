using System.Net;
using DirectoryService.Presentation.Results;
using Shared.Errors;


namespace DirectoryService.Presentation.Middleware;

public class ExceptionsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionsMiddleware> _logger;
    
    public ExceptionsMiddleware(RequestDelegate next, ILogger<ExceptionsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            
            var envelope = Envelope.Error(
                AppErrors.General.SomethingWentWrong()
                    .ToErrors());
            await context.Response.WriteAsJsonAsync(envelope);
        }
    }
}