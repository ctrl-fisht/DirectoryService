using CSharpFunctionalExtensions;
using DirectoryService.Application.Extensions;
using DirectoryService.Application.Repositories;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared.Errors;

namespace DirectoryService.Application.Locations.Create;

public class CreateLocationHandler
{
    private readonly ILocationRepository _repository;
    private readonly IValidator<CreateLocationCommand> _validator;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(ILocationRepository repository, IValidator<CreateLocationCommand> validator, ILogger<CreateLocationHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }
    public async Task<Result<Guid, Errors>> HandleAsync(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        // валидация команды 
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.Errors.ToAppErrors();
        
        // создание доменных объектов
        var timezoneCreateResult = Timezone.Create(command.Request.Timezone);
        
        var addressCreateResult = Address.Create(
            command.Request.Address.Country,
            command.Request.Address.City,
            command.Request.Address.Street,
            command.Request.Address.Building,
            command.Request.Address.RoomNumber);
        

        var nameCreateResult = LocationName.Create(command.Request.Name);
        
        
        // бизнес валидация
        // Проверка на уникальность имени
        var nameExists = await _repository.ExistsByNameAsync(nameCreateResult.Value, cancellationToken);
        if (nameExists)
            return AppErrors.General.AlreadyExists("name").ToErrors();
        
        // Проверка на уникальность адреса
        var addressExists = await _repository.ExistsByAddressAsync(addressCreateResult.Value, cancellationToken);
        if (addressExists)
            return AppErrors.General.AlreadyExists("address").ToErrors();
        
        
        
        // Создание и сохранение в БД
        var locationCreateResult = Location.Create(
            nameCreateResult.Value,
            addressCreateResult.Value,
            timezoneCreateResult.Value);
        
        if (locationCreateResult.IsFailure)
            return locationCreateResult.Error.ToErrors();
        
        var result = await _repository.CreateAsync(locationCreateResult.Value, cancellationToken);
        if (result.IsFailure)
            return result.Error.ToErrors();
        
        _logger.LogInformation(
            "Location {LocationName} was created, id={LocationId}",
            locationCreateResult.Value.LocationName,
            locationCreateResult.Value.Id);
        
        return result.Value;
    }
}