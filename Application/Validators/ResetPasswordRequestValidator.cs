using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty();
        
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8);
    }
}