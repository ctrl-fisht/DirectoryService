using CSharpFunctionalExtensions;
using DirectoryService.Application.Repositories;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace DirectoryService.Application.Locations.Create;

public class CreateLocationHandler
{
    private readonly ILocationRepository _repository;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(ILocationRepository repository, ILogger<CreateLocationHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    public async Task<Result<Guid, Errors>> HandleAsync(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        // в будущем добавить FluentValidation   
        
        
        var timezoneCreateResult = Timezone.Create(command.Timezone);
        if (timezoneCreateResult.IsFailure)
            return timezoneCreateResult.Error.ToErrors();
        
        var addressCreateResult = Address.Create(
            command.Address.Country,
            command.Address.City,
            command.Address.Street,
            command.Address.Building,
            command.Address.RoomNumber);
        
        if (addressCreateResult.IsFailure)
            return addressCreateResult.Error.ToErrors();

        var nameCreateResult = LocationName.Create(command.Name);
        if (nameCreateResult.IsFailure)
            return nameCreateResult.Error.ToErrors();
        
        
        var locationCreateResult = Location.Create(
            nameCreateResult.Value,
            addressCreateResult.Value,
            timezoneCreateResult.Value);
        
        if (locationCreateResult.IsFailure)
            return locationCreateResult.Error.ToErrors();
        
        var result = await _repository.CreateAsync(locationCreateResult.Value, cancellationToken);

        _logger.LogInformation(
            "Location {LocationName} was created, id={LocationId}",
            locationCreateResult.Value.LocationName,
            locationCreateResult.Value.Id);
        
        return result;
    }
}