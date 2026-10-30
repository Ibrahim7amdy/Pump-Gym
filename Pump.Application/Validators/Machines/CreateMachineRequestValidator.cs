using FluentValidation;
using Pump.Application.Dtos.Machines;
using Pump.Application.Validators.Common;

namespace Pump.Application.Validators.Machines;

public class CreateMachineRequestValidator : AbstractValidator<CreateMachineRequestDto>
{
    public CreateMachineRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);

        RuleFor(x => x.MuscleGroup).NotEmpty().MaximumLength(50);

        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500)
            .Must(ValidUrl.BeAValidUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
            .WithMessage("Image URL must be a valid absolute URL.");

        RuleFor(x => x)
            .Must(x => x.ImageFile is not null || !string.IsNullOrWhiteSpace(x.ImageUrl))
            .WithMessage("An image file or a valid image URL is required.");

        RuleFor(x => x.ImageFile)
            .SetValidator(new ImageUploadRequestValidator()!)
            .When(x => x.ImageFile is not null);

        RuleFor(x => x.PinNumber).GreaterThan(0);

        RuleFor(x => x.AlternativeMachineIds)
            .Must(ids => ids is null || (ids.All(id => id > 0) && ids.Distinct().Count() == ids.Count))
            .WithMessage("Alternative machine IDs must be positive and unique.");

        RuleForEach(x => x.ExecutionSteps!)
            .NotEmpty().MaximumLength(200)
            .When(x => x.ExecutionSteps is not null);
    }
}
