using DirectoryService.Application.Extensions;
using DirectoryService.Domain;
using FluentValidation;
using Shared.Errors;

namespace DirectoryService.Application.Positions;

public class CreatePositionValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionValidator()
    {
        const int nameMinLength = Constants.PositionConstants.NameMinLength;
        const int nameMaxLength = Constants.PositionConstants.NameMaxLength;
        const int descriptionMaxLength = Constants.PositionConstants.DescriptionMaxLength;
        
        RuleFor(c => c.Request.Name)
            .NotEmpty()
            .WithMessage(AppErrors.Validation.CannotBeEmpty("name").Serialize())
            .Must(name => 
                name.Length > nameMinLength 
                && name.Length < nameMaxLength)
            .WithMessage(AppErrors.Validation.LengthNotInRange("name",  nameMinLength, nameMaxLength).Serialize());
        
        RuleFor(c => c.Request.Description)
            .Must(description => description?.Length <= descriptionMaxLength)
            .When(c => c.Request.Description != null)
            .WithMessage(AppErrors.Validation.TooLong("description", descriptionMaxLength).Serialize());
        
        RuleFor(c => c.Request.DepartmentIds)
            .Must(departments => departments.Count == departments.Distinct().Count())
            .WithMessage(AppErrors.Validation.DuplicatesInList("departments").Serialize())
            .NotEmpty().WithMessage(AppErrors.Validation.CannotBeEmpty("departments").Serialize());
    }
}