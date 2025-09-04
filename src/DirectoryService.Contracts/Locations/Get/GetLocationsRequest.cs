using DirectoryService.Contracts.Sorting;

namespace DirectoryService.Contracts.Locations.Get;

public record GetLocationsRequest
{
    public List<Guid>? Ids { get; init; } = [];
    public string? Search { get; init; }
    public bool? IsActive { get; init; } = true;
    public string? SortString { get; init; }
    public int? Page { get; init; } = PaginationConstants.DefaultPageIndex;
    public int? PageSize { get; init; } = PaginationConstants.DefaultPageSize;
}