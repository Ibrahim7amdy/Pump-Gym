using FluentValidation;
using Pump.Application.Dtos.Auth;

namespace Pump.Application.Validators.Auth;

public class VerifyResetCodeRequestValidator : AbstractValidator<VerifyResetCodeRequestDto>
{
    public VerifyResetCodeRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Code).NotEmpty().Length(6).Matches("^[0-9]{6}$")
            .WithMessage("Code must be exactly 6 digits.");
    }
}