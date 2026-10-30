using FluentValidation;
using Pump.Application.Dtos.Users;

namespace Pump.Application.Validators.Users;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequestDto>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FullName).MaximumLength(120).When(x => x.FullName is not null);

        RuleFor(x => x.RankTitle).MaximumLength(60).When(x => x.RankTitle is not null);

        RuleFor(x => x.Bio).MaximumLength(280).When(x => x.Bio is not null);

        RuleFor(x => x.TargetWeightKg)
            .InclusiveBetween(40, 300)
            .When(x => x.TargetWeightKg is not null)
            .WithMessage("Target weight must be between 40 and 300 kg.");

        RuleFor(x => x.CurrentWeightKg)
            .InclusiveBetween(40, 300)
            .When(x => x.CurrentWeightKg is not null)
            .WithMessage("Current weight must be between 40 and 300 kg.");

        RuleFor(x => x.HeightCm)
            .InclusiveBetween(50, 200)
            .When(x => x.HeightCm is not null)
            .WithMessage("Height must be between 50 and 200 cm.");

        RuleFor(x => x.CalorieGoal)
            .InclusiveBetween(500, 10000)
            .When(x => x.CalorieGoal is not null)
            .WithMessage("Calorie goal must be between 500 and 10000.");

        RuleFor(x => x.WaterGoalLiters)
            .InclusiveBetween(0, 20)
            .When(x => x.WaterGoalLiters is not null)
            .WithMessage("Water goal must be between 0 and 20 liters.");
    }
}