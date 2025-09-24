namespace DirectoryService.Contracts.Dtos;

public record AddressDto 
{
    public required string Country { get; set; }
    public required string City { get; set; }
    public required string Street { get; set; }
    public required string Building { get; set; }
    public required int RoomNumber { get; set; }
}