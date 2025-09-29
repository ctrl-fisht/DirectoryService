using FluentValidation;
using Shared.Kernel.Errors;

namespace DirectoryService.Application.Departments.UpdateLocations;

public class UpdateDepartmentLocationsValidator : AbstractValidator<UpdateDepartmentLocationsCommand>
{
    public UpdateDepartmentLocationsValidator()
    {
        RuleFor(c => c.DepartmentId)
            .NotEmpty().WithMessage(AppErrors.Validation.CannotBeEmpty("DepartmentId").Serialize());
        
        RuleFor(c => c.Request.LocationIds.ToList())
            .Must(ids => ids.Count == ids.Distinct().Count())
            .WithMessage(AppErrors.Validation.DuplicatesInList("LocationIds").Serialize())
            .NotEmpty().WithMessage(AppErrors.Validation.CannotBeEmpty("LocationIds").Serialize());
    }
}