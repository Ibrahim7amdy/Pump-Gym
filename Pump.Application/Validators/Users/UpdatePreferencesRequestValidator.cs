using FluentValidation;
using Pump.Application.Dtos.Users;

namespace Pump.Application.Validators.Users;

public class UpdatePreferencesRequestValidator : AbstractValidator<UpdatePreferencesRequestDto>
{
    private static readonly string[] AllowedLanguages = { "en", "ar" };
    private static readonly string[] AllowedThemes = { "lime", "crimson", "teal", "emerald", "amber", "slate" };

    public UpdatePreferencesRequestValidator()
    {
        RuleFor(x => x.PreferredLanguage)
            .Must(lang => AllowedLanguages.Contains(lang!.ToLowerInvariant()))
            .When(x => x.PreferredLanguage is not null)
            .WithMessage("Language must be 'en' or 'ar'.");

        RuleFor(x => x.PreferredThemeId)
            .Must(theme => AllowedThemes.Contains(theme!.ToLowerInvariant()))
            .When(x => x.PreferredThemeId is not null)
            .WithMessage("Invalid theme id.");
    }
}