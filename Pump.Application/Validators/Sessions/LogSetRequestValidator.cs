using FluentValidation;
using Pump.Application.Dtos.Sessions;

namespace Pump.Application.Validators.Sessions
{
    public class LogSetRequestValidator : AbstractValidator<LogSetRequestDto>
    {
        public LogSetRequestValidator()
        {
            RuleFor(x => x.SessionStationId).GreaterThan(0);
            RuleFor(x => x.WeightKg).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Reps).GreaterThan(0);
        }
    }
}
