using FluentValidation;
using Pump.Application.Dtos.Users;

namespace Pump.Application.Validators.Users;

public class UpdateAthleteCoachRequestValidator : AbstractValidator<UpdateAthleteCoachRequestDto>
{
    public UpdateAthleteCoachRequestValidator()
    {
        RuleFor(request => request.CoachName)
            .MaximumLength(120)
            .When(request => request.CoachName is not null);
    }
}
