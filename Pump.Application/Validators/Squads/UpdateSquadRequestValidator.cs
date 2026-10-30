using FluentValidation;
using Pump.Application.Dtos.Squads;

namespace Pump.Application.Validators.Squads;

public sealed class UpdateSquadRequestValidator : AbstractValidator<UpdateSquadRequestDto>
{
    public UpdateSquadRequestValidator()
    {
        RuleFor(x => x.Name).Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Name is required.")
            .MaximumLength(60);
    }
}
