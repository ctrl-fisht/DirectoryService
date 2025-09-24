using DirectoryService.Contracts.Dtos;
using DirectoryService.Contracts.Locations.Create;

namespace DirectoryService.Application.Locations.Create;

public record CreateLocationCommand
{
    public CreateLocationRequest Request { get; set; }
}