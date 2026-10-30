using FluentValidation;
using Pump.Application.Dtos.Machines;
using Pump.Application.Validators.Common;

namespace Pump.Application.Validators.Machines;

public class UpdateMachineRequestValidator : AbstractValidator<UpdateMachineRequestDto>
{
    public UpdateMachineRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(120).When(x => x.Name is not null);
        RuleFor(x => x.MuscleGroup).MaximumLength(50).When(x => x.MuscleGroup is not null);
        RuleFor(x => x.Category).MaximumLength(50).When(x => x.Category is not null);
        RuleFor(x => x.ImageUrl)
            .MaximumLength(500)
            .Must(ValidUrl.BeAValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
            .WithMessage("Image URL must be a valid absolute URL.");

        RuleFor(x => x.ImageFile)
            .SetValidator(new ImageUploadRequestValidator()!)
            .When(x => x.ImageFile is not null);
        RuleFor(x => x.PinNumber).GreaterThan(0).When(x => x.PinNumber is not null);

        RuleFor(x => x.AlternativeMachineIds)
            .Must(ids => ids is null || (ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count))
            .WithMessage("Alternative machine IDs must be positive and unique.");

        RuleForEach(x => x.ExecutionSteps!)
            .NotEmpty().MaximumLength(200)
            .When(x => x.ExecutionSteps is not null);
    }
}
