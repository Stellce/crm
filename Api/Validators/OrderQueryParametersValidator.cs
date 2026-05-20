using Api.Dtos;
using FluentValidation;

namespace Api.Validators;

public class OrderQueryParametersValidator : QueryParametersValidator<OrderQueryParameters>
{
    private static readonly HashSet<string> AllowedSortFields =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "customerId",
            "totalAmount",
            "createdAt"
        };

    public OrderQueryParametersValidator()
        : base(AllowedSortFields)
    {
        RuleFor(x => x.MinTotalAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinTotalAmount.HasValue)
            .WithMessage("MinTotalAmount must be not less than 0");

        RuleFor(x => x.MaxTotalAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxTotalAmount.HasValue)
            .WithMessage("MaxTotalAmount must be not less than 0");

        RuleFor(x => x)
            .Must(x => x.MinTotalAmount <= x.MaxTotalAmount)
            .When(x => x.MinTotalAmount.HasValue && x.MaxTotalAmount.HasValue)
            .WithMessage("MinTotalAmount must be less than or equal to MaxTotalAmount.");

        RuleFor(x => x)
            .Must(x => x.CreatedFrom <= x.CreatedTo)
            .When(x => x.CreatedFrom.HasValue && x.CreatedTo.HasValue)
            .WithMessage("CreatedFrom must be less than or equal to CreatedTo.");
    }
}