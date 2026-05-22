using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class QueryParametersValidator<T> : AbstractValidator<T>
    where T : QueryParameters
{

    protected QueryParametersValidator(IEnumerable<string> allowedSortFields)
    {
        var allowedSortFieldsSet = allowedSortFields.ToHashSet(
            StringComparer.OrdinalIgnoreCase
        );

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 1000)
            .WithMessage("PageSize must be between 1 and 1000.");

        RuleFor(x => x.SortBy)
            .Must(sortBy => allowedSortFields.Contains(sortBy))
            .WithMessage("Invalid sort field.");

        RuleFor(x => x.SortDirection)
            .IsInEnum()
            .WithMessage("Invalid sort direction.");

    }
}