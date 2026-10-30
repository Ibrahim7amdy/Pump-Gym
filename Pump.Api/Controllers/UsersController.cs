using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Users;
using Pump.Application.Dtos.Common;
using Pump.Application.Interfaces.Users;
using Pump.Api.Models.Uploads;
using System.IdentityModel.Tokens.Jwt;


namespace Pump.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserProfileService _profileService;
    private readonly IValidator<UpdateProfileRequestDto> _updateProfileValidator;
    private readonly IValidator<SetAvatarUrlRequestDto> _setAvatarUrlValidator;
    private readonly IValidator<UpdatePreferencesRequestDto> _updatePreferencesValidator;
    private readonly IValidator<ImageUploadRequestDto> _imageUploadValidator;

    public UsersController(IUserProfileService profileService, IValidator<UpdateProfileRequestDto> updateProfileValidator, IValidator<SetAvatarUrlRequestDto> setAvatarUrlValidator, IValidator<UpdatePreferencesRequestDto> updatePreferencesValidator, IValidator<ImageUploadRequestDto> imageUploadValidator)
    {
        _profileService = profileService;
        _updateProfileValidator = updateProfileValidator;
        _setAvatarUrlValidator = setAvatarUrlValidator;
        _updatePreferencesValidator = updatePreferencesValidator;
        _imageUploadValidator = imageUploadValidator;
    }

    private int CurrentUserId
    {
        get
        {
            var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                ?? throw new UnauthorizedException("User id claim not found in token.");
            return int.Parse(claim.Value);
        }
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe(CancellationToken ct)
        => Ok(await _profileService.GetMeAsync(CurrentUserId, ct));

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateMe(UpdateProfileRequestDto request, CancellationToken ct)
    {
        var result = await _updateProfileValidator.ValidateAsync(request, ct);
        if (!result.IsValid) throw new ValidationAppException(result.ToDictionary());

        return Ok(await _profileService.UpdateProfileAsync(CurrentUserId, request, ct));
    }

    [HttpGet("me/avatar/presets")]
    public ActionResult<IReadOnlyList<AvatarPresetDto>> GetAvatarPresets()
        => Ok(_profileService.GetAvatarPresets());

    [HttpPut("me/avatar")]
    public async Task<ActionResult<UserDto>> SetAvatarUrl(SetAvatarUrlRequestDto request, CancellationToken ct)
    {
        var result = await _setAvatarUrlValidator.ValidateAsync(request, ct);
        if (!result.IsValid) throw new ValidationAppException(result.ToDictionary());

        return Ok(await _profileService.SetAvatarUrlAsync(CurrentUserId, request, ct));
    }

    [HttpPost("me/avatar")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UserDto>> UploadAvatar(IFormFile? file, CancellationToken ct)
    {
        var validationRequest = await ImageUploadRequestFactory.CreateAsync(file, ct);
        var validation = await _imageUploadValidator.ValidateAsync(validationRequest, ct);
        if (!validation.IsValid) throw new ValidationAppException(validation.ToDictionary());

        await using var stream = file!.OpenReadStream();
        return Ok(await _profileService.UploadAvatarAsync(CurrentUserId, stream, file.FileName, file.ContentType, ct));
    }

    [HttpPut("me/preferences")]
    public async Task<ActionResult<UserDto>> UpdatePreferences(UpdatePreferencesRequestDto request, CancellationToken ct)
    {
        var result = await _updatePreferencesValidator.ValidateAsync(request, ct);
        if (!result.IsValid) throw new ValidationAppException(result.ToDictionary());

        return Ok(await _profileService.UpdatePreferencesAsync(CurrentUserId, request, ct));
    }
}
