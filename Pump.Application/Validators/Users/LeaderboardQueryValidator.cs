using FluentValidation;
using Pump.Application.Dtos.Users;

namespace Pump.Application.Validators.Users;

public sealed class LeaderboardQueryValidator : AbstractValidator<LeaderboardQueryDto>
{
    private static readonly string[] AllowedTiers = ["All", "S", "A", "B", "C"];
    private static readonly string[] AllowedTimeframes = ["week", "month", "alltime"];

    public LeaderboardQueryValidator()
    {
        RuleFor(request => request.Tier)
            .Must(tier => tier is null || AllowedTiers.Contains(tier, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Tier must be All, S, A, B, or C.");
        RuleFor(request => request.Timeframe)
            .Must(timeframe => AllowedTimeframes.Contains(timeframe, StringComparer.Ordinal))
            .WithMessage("Timeframe must be week, month, or alltime, using lowercase letters.");
    }
}
