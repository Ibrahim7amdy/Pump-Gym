using FluentValidation;
using Pump.Application.Dtos.Analytics;

namespace Pump.Application.Validators.Analytics;

public sealed class UpdateDropInPricingValidator : AbstractValidator<DropInPricingDto>
{
    public UpdateDropInPricingValidator()
    {
        RuleFor(request => request.DropInFee)
            .NotNull()
            .InclusiveBetween(0m, 1000000m);
    }
}
