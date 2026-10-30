using FluentValidation;
using Pump.Application.Dtos.Sessions;

namespace Pump.Application.Validators.Sessions
{
    public class ReplaceStationMachineRequestValidator : AbstractValidator<ReplaceStationMachineRequestDto>
    {
        public ReplaceStationMachineRequestValidator()
        {
            RuleFor(x => x.NewMachineId).GreaterThan(0);
        }
    }
}
