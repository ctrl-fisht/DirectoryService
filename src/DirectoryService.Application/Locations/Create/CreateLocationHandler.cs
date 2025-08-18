using CSharpFunctionalExtensions;
using DirectoryService.Application.Repositories;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.ValueObjects;
using Shared.Errors;

namespace DirectoryService.Application.Locations.Create;

public class CreateLocationHandler
{
    private readonly ILocationRepository _repository;

    public CreateLocationHandler(ILocationRepository repository)
    {
        _repository = repository;
    }
    public async Task<Result<Guid, Error>> HandleAsync(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        // в будущем добавить FluentValidation   

        
        var timezoneCreateResult = Timezone.Create(command.Timezone);
        if (timezoneCreateResult.IsFailure)
            return timezoneCreateResult.Error;
        
        var addressCreateResult = Address.Create(
            command.Address.Country,
            command.Address.City,
            command.Address.Street,
            command.Address.Building,
            command.Address.RoomNumber);
        
        if (addressCreateResult.IsFailure)
            return addressCreateResult.Error;

        var nameCreateResult = LocationName.Create(command.Name);
        if (nameCreateResult.IsFailure)
            return nameCreateResult.Error;
        
        
        var locationCreateResult = Location.Create(
            nameCreateResult.Value,
            addressCreateResult.Value,
            timezoneCreateResult.Value);
        
        if (locationCreateResult.IsFailure)
            return locationCreateResult.Error;
        
        var result = await _repository.CreateAsync(locationCreateResult.Value, cancellationToken);

        return result;
    }
}