using DirectoryService.Contracts.Sorting;

namespace DirectoryService.Presentation.Extensions;

public static class SortExtensions
{
    public static List<SortOption> ToSortOptions(this string? sortString)
    {
        if (string.IsNullOrWhiteSpace(sortString))
            return new List<SortOption>();

        var list = new List<SortOption>();

        foreach (var part in sortString.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var pieces = part.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (pieces.Length != 2)
                continue;

            var field = pieces[0].Trim();
            var directionStr = pieces[1].Trim();

            if (!Enum.TryParse<SortDirection>(directionStr, true, out var direction))
                continue;

            list.Add(new SortOption { Field = field, Direction = direction });
        }

        return list;
    }
}