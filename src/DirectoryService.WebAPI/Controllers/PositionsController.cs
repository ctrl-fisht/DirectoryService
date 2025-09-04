using DirectoryService.Application.Positions;
using DirectoryService.Contracts.Positions.Create;
using DirectoryService.Presentation.Results;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers;

[Route("/api/positions")]
public class PositionsController : ApplicationController
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> Create(
        [FromBody] CreatePositionRequest request,
        [FromServices] CreatePositionHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreatePositionCommand()
        {
            Request = request
        };
        return await handler.HandleAsync(command, cancellationToken);
    }
}