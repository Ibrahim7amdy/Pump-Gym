using FluentValidation;
using Pump.Application.Dtos.Protocols;
using Pump.Application.Validators.Common;


namespace Pump.Application.Validators.Protocols;
public class UpdateProtocolRequestValidator : AbstractValidator<UpdateProtocolRequestDto>
{
    private static readonly string[] AllowedDifficulties = { "Beginner", "Intermediate", "Advanced", "AllLevels" };
    private static readonly string[] AllowedIntensities = { "Push", "Pull", "Legs", "Upper", "Rest" };

    public UpdateProtocolRequestValidator()
    {
        RuleFor(x => x.Title).MaximumLength(150).When(x => x.Title is not null);

        RuleFor(x => x.Category).MaximumLength(50).When(x => x.Category is not null);

        RuleFor(x => x.Difficulty)
            .Must(d => AllowedDifficulties.Contains(d!))
            .When(x => x.Difficulty is not null)
            .WithMessage($"Difficulty must be one of: {string.Join(", ", AllowedDifficulties)}.");

        RuleFor(x => x.Price).GreaterThan(0).When(x => x.Price is not null);

        RuleFor(x => x.DurationDays)
            .GreaterThan(0).LessThanOrEqualTo(730)
            .When(x => x.DurationDays is not null);

        RuleFor(x => x.ExpectedGainLabel).MaximumLength(120).When(x => x.ExpectedGainLabel is not null);
        RuleFor(x => x.ExpectedMuscleGainLabel).MaximumLength(120).When(x => x.ExpectedMuscleGainLabel is not null);
        RuleFor(x => x.ExpectedStrengthGainLabel).MaximumLength(120).When(x => x.ExpectedStrengthGainLabel is not null);
        RuleFor(x => x.ExpectedPoints).GreaterThanOrEqualTo(0).When(x => x.ExpectedPoints is not null);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500)
            .Must(ValidUrl.BeAValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
            .WithMessage("Invalid image URL.");

        When(x => x.Days is not null, () =>
        {
            RuleForEach(x => x.Days!).ChildRules(day =>
            {
                day.RuleFor(d => d.DayNumber)
                    .GreaterThan(0)
                    .When(d => d.DayNumber is not null);

                day.RuleFor(d => d.Label).NotEmpty().MaximumLength(60);

                day.RuleFor(d => d.Intensity)
                    .Must(i => AllowedIntensities.Contains(i))
                    .WithMessage($"Intensity must be one of: {string.Join(", ", AllowedIntensities)}.");


                day.RuleForEach(d => d.Exercises).ChildRules(ex =>
                {
                    ex.RuleFor(e => e.MachineId).GreaterThan(0);

                    ex.RuleFor(e => e.Sets).GreaterThan(0);

                    ex.RuleFor(e => e.RepsLabel).NotEmpty().MaximumLength(20);

                    ex.RuleFor(e => e.TargetWeightKg).GreaterThanOrEqualTo(0);

                    ex.RuleFor(e => e.RestSeconds).GreaterThanOrEqualTo(0);
                });
            });

            RuleFor(x => x.Days!)
                .Must(days => days.Count == 0
                    || days.All(day => day.DayNumber.HasValue)
                    || days.All(day => !day.DayNumber.HasValue))
                .WithMessage("Protocol days must either all have a day number or all be unnumbered for an open protocol.");

            RuleFor(x => x.Days!)
                .Must(days =>
                {
                    var numbered = days.Where(d => d.DayNumber is not null).Select(d => d.DayNumber);
                    return numbered.Distinct().Count() == numbered.Count();
                })
                .WithMessage("Day numbers must be unique among numbered days.");
        });

        When(x => x.WeeklyLoadPlan is not null, () =>
        {
            RuleForEach(x => x.WeeklyLoadPlan!).ChildRules(week =>
            {
                week.RuleFor(w => w.WeekNumber).GreaterThan(0);
                week.RuleFor(w => w.PhaseLabel).NotEmpty().MaximumLength(80);
                week.RuleFor(w => w.LoadPercent).GreaterThan(0).LessThanOrEqualTo(200);
            });

            RuleFor(x => x.WeeklyLoadPlan!)
                .Must((request, weeks) => weeks.Select(week => week.WeekNumber).Distinct().Count() == weeks.Count
                    && (request.DurationDays is null
                        || weeks.All(week => week.WeekNumber <= (int)Math.Ceiling(request.DurationDays.Value / 7.0))))
                .WithMessage("Weekly load plan weeks must be unique and within the protocol duration.");
        });
    }
}
