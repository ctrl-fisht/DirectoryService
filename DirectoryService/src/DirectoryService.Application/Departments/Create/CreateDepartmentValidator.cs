using DirectoryService.Application.Extensions;
using DirectoryService.Domain.ValueObjects;
using FluentValidation;
using Shared.Errors;

namespace DirectoryService.Application.Departments.Create;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentValidator()
    {
        RuleFor(c => c.Request.Name).CanMakeValueObject(DepartmentName.Create);
        RuleFor(c => c.Request.Identifier).CanMakeValueObject(Identifier.Create);
        
        RuleFor(c => c.Request.Locations)
            .Must(locations => locations.Count == locations.Distinct().Count())
            .WithMessage(AppErrors.Validation.DuplicatesInList("locations").Serialize())
            .NotEmpty().WithMessage(AppErrors.Validation.CannotBeEmpty("locations").Serialize());
    }
}