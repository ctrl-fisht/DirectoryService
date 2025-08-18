using DirectoryService.Application.Locations.Create;
using DirectoryService.Contracts.Locations.Create;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[Route("locations")]
public class LocationsController : ApplicationController
{
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
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
        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }
}