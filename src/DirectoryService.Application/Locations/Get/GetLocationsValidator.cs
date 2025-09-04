using System.Data;
using FluentValidation;
using Shared.Errors;

namespace DirectoryService.Application.Locations.Get;

public class GetLocationsValidator : AbstractValidator<GetLocationsQuery>
{
    public GetLocationsValidator()
    {
        RuleFor(q => q.Page).GreaterThan(0)
            .WithMessage(AppErrors.Validation.MustBeGreaterThan("page",0).Serialize());
        RuleFor(q => q.PageSize).GreaterThan(0)
            .WithMessage(AppErrors.Validation.MustBeGreaterThan("pageSize",0).Serialize());
    }
}