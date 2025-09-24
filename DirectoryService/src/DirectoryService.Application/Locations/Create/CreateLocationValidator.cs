using DirectoryService.Application.Extensions;
using DirectoryService.Domain;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using Shared.Errors;

namespace DirectoryService.Application.Locations.Create;

public class CreateLocationValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationValidator()
    {
        RuleFor(command => command.Request.Name)
            .CanMakeValueObject(LocationName.Create);

        RuleFor(command => command.Request.Timezone)
            .CanMakeValueObject(Timezone.Create);

        RuleFor(command => command.Request.Address)
            .NotEmpty()
            .Custom((addressDto, context) =>
            {
                var result = Address.Create(
                    addressDto.Country,
                    addressDto.City,
                    addressDto.Street,
                    addressDto.Building,
                    addressDto.RoomNumber);
        
                if (result.IsFailure)
                    context.AddFailure(result.Error.Serialize());
            });
    }
}