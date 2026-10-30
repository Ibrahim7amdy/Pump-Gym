using FluentValidation;
using Pump.Application.Dtos.Squads;

namespace Pump.Application.Validators.Squads;

public sealed class AddSquadMemberRequestValidator : AbstractValidator<AddSquadMemberRequestDto>
{
    public AddSquadMemberRequestValidator()
    {
        RuleFor(x => x).Must(request =>
                !string.IsNullOrWhiteSpace(request.Username) ^ !string.IsNullOrWhiteSpace(request.QrPayload))
            .WithMessage("Provide either a username or a QR payload.");
        RuleFor(x => x.Username).MaximumLength(30).When(x => x.Username is not null);
        RuleFor(x => x.QrPayload).MaximumLength(100).When(x => x.QrPayload is not null);
    }
}
