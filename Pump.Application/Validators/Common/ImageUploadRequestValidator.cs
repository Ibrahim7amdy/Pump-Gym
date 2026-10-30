using FluentValidation;
using Pump.Application.Dtos.Common;

namespace Pump.Application.Validators.Common;

public sealed class ImageUploadRequestValidator : AbstractValidator<ImageUploadRequestDto>
{
    private const long MaximumLength = 5 * 1024 * 1024;

    public ImageUploadRequestValidator()
    {
        RuleFor(image => image.FileName)
            .NotEmpty()
            .WithMessage("An image file is required.");

        RuleFor(image => image.Length)
            .GreaterThan(0)
            .WithMessage("The image file cannot be empty.")
            .LessThanOrEqualTo(MaximumLength)
            .WithMessage("Image size must not exceed 5 MB.");

        RuleFor(image => image.ContentType)
            .Must(IsSupportedContentType)
            .WithMessage("Only JPEG, PNG, and WebP images are supported.");

        RuleFor(image => image)
            .Must(HaveMatchingExtension)
            .WithMessage("The image file extension must match its content type.");

        RuleFor(image => image)
            .Must(HaveValidSignature)
            .WithMessage("The uploaded file content is not a valid JPEG, PNG, or WebP image.");
    }

    private static bool IsSupportedContentType(string? contentType)
        => contentType?.ToLowerInvariant() is "image/jpeg" or "image/png" or "image/webp";

    private static bool HaveMatchingExtension(ImageUploadRequestDto image)
    {
        var extension = Path.GetExtension(image.FileName)?.ToLowerInvariant();
        return (image.ContentType?.ToLowerInvariant(), extension) switch
        {
            ("image/jpeg", ".jpg" or ".jpeg") => true,
            ("image/png", ".png") => true,
            ("image/webp", ".webp") => true,
            _ => false
        };
    }

    private static bool HaveValidSignature(ImageUploadRequestDto image)
    {
        var header = image.Header;
        return image.ContentType?.ToLowerInvariant() switch
        {
            "image/jpeg" => header.Length >= 3
                && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            "image/png" => header.Length >= 8
                && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
            "image/webp" => header.Length >= 12
                && header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F'
                && header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P',
            _ => false
        };
    }
}
