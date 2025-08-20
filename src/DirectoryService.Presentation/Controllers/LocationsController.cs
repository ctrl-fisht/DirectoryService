using DirectoryService.Application.Locations.Create;
using DirectoryService.Contracts.Locations.Create;
using DirectoryService.Presentation.Results;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[Route("locations")]
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
            Name = request.Name,
            Timezone = request.Timezone,
            Address = request.Address
        };
        
        var result = await handler.HandleAsync(command, cancellationToken);
        
        // В будущем добавить envelope
        return result;
    }
}