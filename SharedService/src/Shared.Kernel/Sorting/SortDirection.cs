using System.Text.Json.Serialization;

namespace Shared.Kernel.Sorting;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SortDirection
{
    Asc,
    Desc
}