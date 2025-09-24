namespace DirectoryService.Contracts.Dtos;

public record LocationDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Timezone { get; init; }
    public required DateTime CreatedUtc { get; init; }
}