namespace Shared.Kernel.Sorting;

public record SortOption
{
    public SortDirection Direction { get; init; }
    public required string Field { get; init; }
}