using Api.Dtos;
using FluentValidation;

namespace Api.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("Customer Id must be greater han 0.");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0)
            .WithMessage("Total Amount must be greater than 0.");
    }
}