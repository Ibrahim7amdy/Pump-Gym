using FluentValidation;
using Pump.Application.Dtos.Visits;

namespace Pump.Application.Validators.Visits;

public class CreateWalkInVisitRequestValidator : AbstractValidator<CreateWalkInVisitRequestDto>
{
    public CreateWalkInVisitRequestValidator()
    {
        RuleFor(x => x.GuestName)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Visitor name is required.")
            .MaximumLength(120);
    }
}
