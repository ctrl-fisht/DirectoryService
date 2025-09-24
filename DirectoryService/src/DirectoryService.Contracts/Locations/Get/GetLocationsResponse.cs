using DirectoryService.Contracts.Dtos;

namespace DirectoryService.Contracts.Locations.Get;

public record GetLocationsResponse
{
    public List<LocationDto> Locations { get; init; } = new List<LocationDto>();
    public long TotalCount { get; init; }
}