using DirectoryService.Application.Departments.Create;
using DirectoryService.Contracts.Departments.Create;
using DirectoryService.Presentation.Results;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[Route("/api/departments")]
public class DepartmentsController : ApplicationController
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromBody] CreateDepartmentRequest request,
        [FromServices] CreateDepartmentHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateDepartmentCommand()
        {
            Request = request
        };

        return await handler.HandleAsync(command, cancellationToken);
    }
}