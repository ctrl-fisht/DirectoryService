using DirectoryService.Contracts.Dtos;

namespace DirectoryService.Application.Locations.Create;

public record CreateLocationCommand
{
    public required string Name { get; set; }
    public required AddressDto Address { get; set; }
    public required string Timezone { get; set; }
}