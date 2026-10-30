using FluentValidation;
using Pump.Application.Dtos.Sessions;

namespace Pump.Application.Validators.Sessions
{
    public class AddStationRequestValidator : AbstractValidator<AddStationRequestDto>
    {
        public AddStationRequestValidator()
        {
            RuleFor(x => x.MachineId).GreaterThan(0);
            RuleFor(x => x.TargetSets).GreaterThan(0);
            RuleFor(x => x.RestSeconds).GreaterThanOrEqualTo(0);
        }
    }
}
