using DirectoryService.Contracts;
using DirectoryService.Contracts.Locations.Get;
using DirectoryService.Contracts.Sorting;

namespace DirectoryService.Application.Locations.Get;

public record GetLocationsQuery
{
    public required List<Guid> Ids { get; init; }
    public string? Search { get; init; }
    public required bool IsActive { get; init; }
    public List<SortOption>? SortOptions { get; init; }
    public int Page { get; init; } = PaginationConstants.DefaultPageIndex;
    public int PageSize { get; init; } = PaginationConstants.DefaultPageSize;
}