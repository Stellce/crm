using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

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