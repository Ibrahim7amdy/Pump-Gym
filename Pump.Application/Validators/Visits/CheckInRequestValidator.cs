using FluentValidation;
using Pump.Application.Dtos.Visits;

namespace Pump.Application.Validators.Visits;

public class CheckInRequestValidator : AbstractValidator<CheckInRequestDto>
{
    public CheckInRequestValidator()
    {
        RuleFor(x => x.Payload).NotEmpty().MaximumLength(300);
    }
}
