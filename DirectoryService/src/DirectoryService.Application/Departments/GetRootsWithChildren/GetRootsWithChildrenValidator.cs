using DirectoryService.Contracts;
using FluentValidation;
using Shared.Kernel.Errors;

namespace DirectoryService.Application.Departments.GetRootsWithChildren;

public class GetRootsWithChildrenValidator : AbstractValidator<GetRootsWithChildrenQuery>
{
    public GetRootsWithChildrenValidator()
    {
        RuleFor(q => q.Page).GreaterThan(0)
            .WithMessage(AppErrors.Validation.MustBeGreaterThan("page",0).Serialize());
        
        RuleFor(q => q.PageSize).GreaterThan(0)
            .WithMessage(AppErrors.Validation.MustBeGreaterThan("pageSize",0).Serialize());
        RuleFor(q => q.PageSize).LessThan(PaginationConstants.MaxPageSize)
            .WithMessage(AppErrors.Validation.LengthNotInRange(
                "pageSize", 1, PaginationConstants.MaxPageSize).Serialize());
        
        RuleFor(q => q.Prefetch).GreaterThanOrEqualTo(0)
            .WithMessage(AppErrors.Validation.MustBeGreaterOrEqualThan("prefetch",0).Serialize());
        RuleFor(q => q.Prefetch).LessThan(PaginationConstants.MaxPrefetch)
            .WithMessage(AppErrors.Validation.LengthNotInRange(
                "prefetch", 0, PaginationConstants.MaxPrefetch).Serialize());
    }
}