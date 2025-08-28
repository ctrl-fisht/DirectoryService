using FluentValidation;
using Shared.Errors;

namespace DirectoryService.Application.Departments.UpdateParent;

public class UpdateDepartmentParentValidator : AbstractValidator<UpdateDepartmentParentCommand>
{
    public UpdateDepartmentParentValidator()
    {
        RuleFor(command => command.DepartmentId).NotEmpty()
            .WithMessage(AppErrors.Validation.CannotBeEmpty("departmentId").Serialize());
        
        RuleFor(c => new { ParentId = c.Request.ParentId, DepartmentId = c.DepartmentId })
            .Must(items => items.DepartmentId != items.ParentId)
            .WithMessage(AppErrors.Hierarchy.CannotAddSelfAsAParent().Serialize());
    }
}