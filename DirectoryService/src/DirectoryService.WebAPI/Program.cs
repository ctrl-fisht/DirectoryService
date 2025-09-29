using DirectoryService.Application;
using DirectoryService.Infrastructure;
using Serilog;
using Shared.Framework;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration);
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionsMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "DirectoryServiceAPI");
    });
}

app.MapControllers();

app.Run();

namespace DirectoryService.Presentation
{
    public partial class Program { }
}