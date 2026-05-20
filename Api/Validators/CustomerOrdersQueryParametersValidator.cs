using Api.Dtos;
using FluentValidation;

namespace Api.Validators;

public class CustomerOrdersQueryParametersValidator : QueryParametersValidator<CustomerOrdersQueryParameters>
{
    private static readonly HashSet<string> AllowedSortFields =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "id",
            "totalAmount",
            "createdAt"
        };
    public CustomerOrdersQueryParametersValidator() : base(AllowedSortFields)
    {
        RuleFor(x => x.MinTotalAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinTotalAmount.HasValue)
            .WithMessage("MinTotalAmount must be greater or equal to 0.");

        RuleFor(x => x.MaxTotalAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxTotalAmount.HasValue)
            .WithMessage("MaxTotalAmount must be greater or equal to 0.");

        RuleFor(x => x)
            .Must(x => x.MaxTotalAmount >= x.MinTotalAmount)
            .When(x => x.MaxTotalAmount.HasValue && x.MinTotalAmount.HasValue)
            .WithMessage("MaxTotalAmount must be greater than or equal to MinTotalAmount");

        RuleFor(x => x)
            .Must(x => x.CreatedFrom <= x.CreatedTo)
            .When(x => x.CreatedFrom.HasValue && x.CreatedTo.HasValue)
            .WithMessage("CreatedFrom must be lesser or equal to CreatedTo.");
    }
}