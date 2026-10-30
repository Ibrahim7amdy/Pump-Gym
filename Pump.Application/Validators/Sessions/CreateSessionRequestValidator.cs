using FluentValidation;
using Pump.Application.Dtos.Sessions;

namespace Pump.Application.Validators.Sessions
{
    public class CreateSessionRequestValidator : AbstractValidator<CreateSessionRequestDto>
    {
        public CreateSessionRequestValidator()
        {
            RuleFor(x => x.ProtocolDayId).GreaterThan(0).When(x => x.ProtocolDayId is not null);
        }
    }
}
