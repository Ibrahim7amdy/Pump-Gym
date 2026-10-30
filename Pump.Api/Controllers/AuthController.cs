using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Auth;
using Pump.Application.Dtos.Common;
using Pump.Application.Interfaces.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Pump.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<ResetPasswordRequestDto> _resetPasswordValidator;
    private readonly IValidator<RefreshRequestDto> _refreshValidator;
    private readonly IValidator<LogoutRequestDto> _logoutValidator;
    private readonly IValidator<ForgotPasswordRequestDto> _forgotPasswordValidator;
    private readonly IValidator<VerifyResetCodeRequestDto> _verifyResetCodeValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterRequestDto> registerValidator,
        IValidator<LoginRequestDto> loginValidator,
        IValidator<ResetPasswordRequestDto> resetPasswordValidator,
        IValidator<RefreshRequestDto> refreshValidator,
        IValidator<LogoutRequestDto> logoutValidator,
        IValidator<ForgotPasswordRequestDto> forgotPasswordValidator,
        IValidator<VerifyResetCodeRequestDto> verifyResetCodeValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _refreshValidator = refreshValidator;
        _logoutValidator = logoutValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _verifyResetCodeValidator = verifyResetCodeValidator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request, CancellationToken ct)
    {
        var result = await _registerValidator.ValidateAsync(request, ct);
        if (!result.IsValid)
            throw new ValidationAppException(result.ToDictionary());

        return Ok(await _authService.RegisterAsync(request, ct));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request, CancellationToken ct)
    {
        var result = await _loginValidator.ValidateAsync(request, ct);
        if (!result.IsValid)
            throw new ValidationAppException(result.ToDictionary());

        return Ok(await _authService.LoginAsync(request, ct));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshRequestDto request, CancellationToken ct)
    {
        var result = await _refreshValidator.ValidateAsync(request, ct);
        if (!result.IsValid)
            throw new ValidationAppException(result.ToDictionary());

        return Ok(await _authService.RefreshAsync(request, ct));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequestDto request, CancellationToken ct)
    {
        var result = await _logoutValidator.ValidateAsync(request, ct);
        if (!result.IsValid)
            throw new ValidationAppException(result.ToDictionary());

        var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        await _authService.LogoutAsync(userId, request, ct);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<MessageResponseDto>> ForgotPassword(ForgotPasswordRequestDto request, CancellationToken ct)
    {
        var result = await _forgotPasswordValidator.ValidateAsync(request, ct);
        if (!result.IsValid)
            throw new ValidationAppException(result.ToDictionary());

        await _authService.ForgotPasswordAsync(request, ct);
        return Ok(new MessageResponseDto("If an account exists for this email, a reset code has been sent."));
    }

    [HttpPost("verify-reset-code")]
    public async Task<ActionResult<VerifyResetCodeResponseDto>> VerifyResetCode(VerifyResetCodeRequestDto request, CancellationToken ct)
    {
        var result = await _verifyResetCodeValidator.ValidateAsync(request, ct);
        if (!result.IsValid)
            throw new ValidationAppException(result.ToDictionary());

        return Ok(await _authService.VerifyResetCodeAsync(request, ct));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<MessageResponseDto>> ResetPassword(ResetPasswordRequestDto request, CancellationToken ct)
    {
        var result = await _resetPasswordValidator.ValidateAsync(request, ct);
        if (!result.IsValid) throw new ValidationAppException(result.ToDictionary());

        await _authService.ResetPasswordAsync(request, ct);
        return Ok(new MessageResponseDto("Password has been reset successfully."));
    }
}