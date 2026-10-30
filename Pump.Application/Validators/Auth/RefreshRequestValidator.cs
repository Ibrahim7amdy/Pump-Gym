using FluentValidation;
using Pump.Application.Dtos.Auth;

namespace Pump.Application.Validators.Auth;

public class RefreshRequestValidator : AbstractValidator<RefreshRequestDto>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}