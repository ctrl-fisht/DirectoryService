using DirectoryService.Contracts.Dtos;

namespace DirectoryService.Contracts.Locations.Create;

public record CreateLocationRequest
{
    public required string Name { get; set; }
    public required AddressDto  Address { get; set; }
    public required string Timezone { get; set; }
}