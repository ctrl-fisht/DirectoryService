namespace DirectoryService.Contracts.Sorting;

public record SortOption
{
    public SortDirection Direction { get; init; }
    public string Field { get; init; }
}