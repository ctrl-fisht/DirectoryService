using FluentValidation;
using Shared.Errors;

namespace DirectoryService.Application.Departments.Deactivate;

public class DeactivateDepartmentValidator : AbstractValidator<DeactivateDepartmentCommand>
{
    public DeactivateDepartmentValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage(AppErrors.Validation.CannotBeEmpty("id").Serialize());
    }
}