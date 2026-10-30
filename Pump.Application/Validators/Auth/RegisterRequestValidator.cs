using FluentValidation;
using Pump.Application.Dtos.Auth;

namespace Pump.Application.Validators.Auth
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);

            RuleFor(x => x.Username).NotEmpty().MaximumLength(30)
                .Matches("^[a-z0-9._]+$").WithMessage("Username must be lowercase letters, numbers, dots or underscores only.");

            RuleFor(x => x.Password)
             .NotEmpty()
             .MinimumLength(8)
             .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
             .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
             .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
             .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.FullName).NotEmpty().MaximumLength(120);
        }
    }
}
