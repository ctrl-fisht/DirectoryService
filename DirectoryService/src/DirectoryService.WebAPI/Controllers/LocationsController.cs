using DirectoryService.Application.Locations.Create;
using DirectoryService.Application.Locations.Get;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Locations.Create;
using DirectoryService.Contracts.Locations.Get;
using Microsoft.AspNetCore.Mvc;
using Shared.Framework;
using Shared.Framework.Results;
using Shared.Framework.SortingExtensions;

namespace DirectoryService.Presentation.Controllers;

[Route("/api/locations")]
public class LocationsController : ApplicationController
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromBody] CreateLocationRequest request,
        [FromServices] CreateLocationHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateLocationCommand()
        {
            Request = request
        };
        
        var result = await handler.HandleAsync(command, cancellationToken);
        
        // В будущем добавить envelope
        return result;
    }

    [HttpGet]
    public async Task<EndpointResult<GetLocationsResponse>> Get(
        [FromQuery] GetLocationsRequest request,
        [FromServices] GetLocationsHandler handler,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLocationsQuery()
        {
            Ids = request.Ids ?? [],
            Search = request.Search,
            IsActive = request.IsActive ?? true,
            SortOptions = request.SortString == null ? null : request.SortString.ToSortOptions(),
            Page = request.Page ?? PaginationConstants.DefaultPageIndex,
            PageSize = request.PageSize ?? PaginationConstants.DefaultPageSize
        };
        return await handler.HandleAsync(query, cancellationToken);
    }
}