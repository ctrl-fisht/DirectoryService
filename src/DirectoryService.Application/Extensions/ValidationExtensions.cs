using CSharpFunctionalExtensions;
using FluentValidation;
using FluentValidation.Results;
using Shared.Errors;

namespace DirectoryService.Application.Extensions;

public static class ValidationExtensions
{
    public static IRuleBuilderOptionsConditions<T, TElement> CanMakeValueObject<T, TElement, TValueObject>
    (this IRuleBuilder<T, TElement> ruleBuilder, Func<TElement, Result<TValueObject, Error>> factoryMethod)
    {
        return ruleBuilder.Custom((value, context) =>
        {
            Result<TValueObject, Error> result = factoryMethod(value);

            if (!result.IsFailure)
                return;
            
            context.AddFailure(result.Error.Serialize());
        });
    }

    public static Errors ToAppErrors(this IEnumerable<ValidationFailure> failures)
    {
        return new Errors(failures.Select(failure => Error.Deserialize(failure.ErrorMessage)));
    }
}