using DirectoryService.Application.Departments.Create;
using DirectoryService.Application.Departments.MoveDepartment;
using DirectoryService.Application.Departments.UpdateParent;
using DirectoryService.Contracts.Departments.Create;
using DirectoryService.Contracts.Departments.UpdateLocations;
using DirectoryService.Contracts.Departments.UpdateParent;
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

    [Route("{departmentId:Guid}/locations")]
    [HttpPut]
    public async Task<EndpointResult<Guid>> UpdateDepartmentLocations(
        [FromRoute] Guid departmentId,
        [FromBody] UpdateDepartmentLocationsRequest request,
        [FromServices] MoveDepartmentHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new MoveDepartmentCommand()
        {
            DepartmentId = departmentId,
            Request = request
        };
        return await handler.HandleAsync(command, cancellationToken);
    }

    [Route("{departmentId:Guid}/parent")]
    [HttpPut]
    public async Task<EndpointResult<Guid>> MoveDepartmentParent(
        [FromRoute] Guid departmentId,
        [FromBody] UpdateDepartmentParentRequest request,
        [FromServices] UpdateDepartmentParentHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateDepartmentParentCommand()
        {
            DepartmentId = departmentId,
            Request = request
        };

        return await handler.HandleAsync(command, cancellationToken);
    }
}

