using DirectoryService.Contracts.Positions.Create;

namespace DirectoryService.Application.Positions;

public record CreatePositionCommand
{
    public CreatePositionRequest Request { get; set; }
}