using FluentValidation;
using Shared.Kernel.Errors;

namespace DirectoryService.Application.Departments.MoveDepartment;

public class MoveDepartmentValidator : AbstractValidator<MoveDepartmentCommand>
{
    public MoveDepartmentValidator()
    {
        RuleFor(command => command.DepartmentId).NotEmpty()
            .WithMessage(AppErrors.Validation.CannotBeEmpty("departmentId").Serialize());
        
        RuleFor(c => new { ParentId = c.Request.ParentId, DepartmentId = c.DepartmentId })
            .Must(items => items.DepartmentId != items.ParentId)
            .WithMessage(AppErrors.Hierarchy.CannotAddSelfAsAParent().Serialize());
    }
}