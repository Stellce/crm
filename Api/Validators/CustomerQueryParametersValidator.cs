using Api.Dtos;
using FluentValidation;

namespace Api.Validators;

public class CustomerQueryParametersValidator : QueryParametersValidator<CustomerQueryParameters>
{
    private static readonly HashSet<string> AllowedSortFields =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "id",
            "name",
            "email",
            "createdAt"
        };

    public CustomerQueryParametersValidator()
        : base(AllowedSortFields)
    {
    }
}