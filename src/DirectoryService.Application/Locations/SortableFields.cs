namespace DirectoryService.Application.Locations;

public static partial class SortableFields
{
    public static Dictionary<string, string> Location = new()
    {
      ["name"] = "name",
      ["date"] = "created_at"
    };
}