using FluentValidation;
using Pump.Application.Dtos.Users;
using Pump.Application.Validators.Common;

namespace Pump.Application.Validators.Users;

public class SetAvatarUrlRequestValidator : AbstractValidator<SetAvatarUrlRequestDto>
{
    public SetAvatarUrlRequestValidator()
    {
        RuleFor(x => x.AvatarUrl)
            .NotEmpty()
            .MaximumLength(500)
            .Must(ValidUrl.BeAValidUrl).WithMessage("Avatar URL must be a valid absolute URL.");
    }
}