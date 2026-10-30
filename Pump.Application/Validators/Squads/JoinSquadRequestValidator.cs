using FluentValidation;
using Pump.Application.Dtos.Squads;

namespace Pump.Application.Validators.Squads;

public sealed class JoinSquadRequestValidator : AbstractValidator<JoinSquadRequestDto>
{
    public JoinSquadRequestValidator()
        => RuleFor(x => x.InviteCode).Must(code => !string.IsNullOrWhiteSpace(code)).WithMessage("Invite code is required.")
            .MaximumLength(12);
}
