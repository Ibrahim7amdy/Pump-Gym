using FluentValidation;
using Pump.Application.Dtos.Protocols;

namespace Pump.Application.Validators.Protocols;

public class AssignProtocolRequestValidator : AbstractValidator<AssignProtocolRequestDto>
{
    public AssignProtocolRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}