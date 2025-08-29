using FluentValidation;
using Shared.Errors;

namespace DirectoryService.Application.Departments.MoveDepartment;

public class MoveDepartmentValidator : AbstractValidator<MoveDepartmentCommand>
{
    public MoveDepartmentValidator()
    {
        RuleFor(c => c.DepartmentId)
            .NotEmpty().WithMessage(AppErrors.Validation.CannotBeEmpty("DepartmentId").Serialize());
        
        RuleFor(c => c.Request.LocationIds.ToList())
            .Must(ids => ids.Count == ids.Distinct().Count())
            .WithMessage(AppErrors.Validation.DuplicatesInList("LocationIds").Serialize())
            .NotEmpty().WithMessage(AppErrors.Validation.CannotBeEmpty("LocationIds").Serialize());
    }
}