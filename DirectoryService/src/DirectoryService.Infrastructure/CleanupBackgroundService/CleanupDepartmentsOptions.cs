namespace DirectoryService.Infrastructure.CleanupBackgroundService;

public record CleanupDepartmentsOptions
{
    public int InactiveDays { get; init; }
    public int IntervalHours { get; init; }
    public string? DailyTimeUtc { get; init; }
}