using System.Text.Json.Serialization;

namespace DirectoryService.Contracts.Sorting;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortDirection
{
    Asc,
    Desc
}