using DirectoryService.Contracts;
using FluentValidation;
using Shared.Errors;

namespace DirectoryService.Application.Departments.GetDepartmentChildren;

public class GetDepartmentChildrenValidator : AbstractValidator<GetDepartmentChildrenQuery>
{
    public GetDepartmentChildrenValidator()
    {
        RuleFor(q => q.Page).GreaterThan(0)
            .WithMessage(AppErrors.Validation.MustBeGreaterThan("page",0).Serialize());
        
        RuleFor(q => q.PageSize).GreaterThan(0)
            .WithMessage(AppErrors.Validation.MustBeGreaterThan("pageSize",0).Serialize());
        RuleFor(q => q.PageSize).LessThan(PaginationConstants.MaxPageSize)
            .WithMessage(AppErrors.Validation.LengthNotInRange(
                "pageSize", 1, PaginationConstants.MaxPageSize).Serialize());
    }
}