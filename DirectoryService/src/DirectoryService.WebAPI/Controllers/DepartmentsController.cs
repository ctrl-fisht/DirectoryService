using DirectoryService.Application.Departments.Create;
using DirectoryService.Application.Departments.Deactivate;
using DirectoryService.Application.Departments.GetDepartmentChildren;
using DirectoryService.Application.Departments.GetRootsWithChildren;
using DirectoryService.Application.Departments.GetTopPositions;
using DirectoryService.Application.Departments.MoveDepartment;
using DirectoryService.Application.Departments.UpdateLocations;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments.Create;
using DirectoryService.Contracts.Departments.GetChildren;
using DirectoryService.Contracts.Departments.GetRootsWithChildren;
using DirectoryService.Contracts.Departments.GetTopPositions;
using DirectoryService.Contracts.Departments.MoveDepartment;
using DirectoryService.Contracts.Departments.UpdateLocations;
using Shared.Framework.Results;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework;

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
        [FromServices] UpdateDepartmentLocationsHandler locationsHandler,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateDepartmentLocationsCommand()
        {
            DepartmentId = departmentId,
            Request = request
        };
        return await locationsHandler.HandleAsync(command, cancellationToken);
    }

    [Route("{departmentId:Guid}/parent")]
    [HttpPut]
    public async Task<EndpointResult<Guid>> MoveDepartmentParent(
        [FromRoute] Guid departmentId,
        [FromBody] MoveDepartmentRequest request,
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

    [Route("top-positions")]
    [HttpGet]
    public async Task<EndpointResult<GetTopPositionsResponse>> TopPositions(
        [FromServices] GetTopPositionsHandler handler,
        CancellationToken cancellationToken = default)
    {
        return await handler.HandleAsync(cancellationToken);
    }

    [Route("roots")]
    [HttpGet]
    public async Task<EndpointResult<GetRootsWithChildrenResponse>> GetRootsWithChildren(
        [FromQuery] GetRootsWithChildrenRequest request,
        [FromServices] GetRootsWithChildrenHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRootsWithChildrenQuery()
        {
            Page = request.Page ?? PaginationConstants.DefaultPageIndex,
            PageSize = request.PageSize ?? PaginationConstants.DefaultPageSize,
            Prefetch = request.Prefetch ?? PaginationConstants.DefaultPrefetch,
        };
        return await handler.HandleAsync(query, cancellationToken);
    }

    [Route("{parentId:Guid}/children")]
    [HttpGet]
    public async Task<EndpointResult<GetDepartmentChildrenResponse>> GetDepartmentChildren(
        [FromRoute] Guid parentId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromServices] GetDepartmentChildrenHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDepartmentChildrenQuery()
        {
            ParentId = parentId,
            Page = page ?? PaginationConstants.DefaultPageIndex,
            PageSize = pageSize ?? PaginationConstants.DefaultPageSize,
        };
        return await handler.HandleAsync(query, cancellationToken);
    }

    [Route("{id:Guid}")]
    [HttpDelete]
    public async Task<EndpointResult<Guid>> Deactivate(
        [FromRoute] Guid id,
        [FromServices] DeactivateDepartmentHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new DeactivateDepartmentCommand()
        {
            Id = id
        };
        
        return await handler.HandleAsync(command, cancellationToken);
    }
}

