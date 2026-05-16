using Api.Dtos;
using FluentValidation;

namespace Api.Validators;

public class PatchCustomerRequestValidator : AbstractValidator<PatchCustomerRequest>
{
    public PatchCustomerRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Name != null || x.Email != null)
            .WithMessage("At least one of Name or Email must be provided.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters.")
            .When(x => x.Name != null);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters.")
            .When(x => x.Email != null);
    }
}