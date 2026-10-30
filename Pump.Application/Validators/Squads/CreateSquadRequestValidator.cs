using FluentValidation;
using Pump.Application.Dtos.Squads;

namespace Pump.Application.Validators.Squads;

public sealed class CreateSquadRequestValidator : AbstractValidator<CreateSquadRequestDto>
{
    public CreateSquadRequestValidator()
    {
        RuleFor(x => x.Name).Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Name is required.")
            .MaximumLength(60);
        RuleFor(x => x.MonthlyGoalTonnage).GreaterThan(0).LessThanOrEqualTo(1000000);
    }
}
