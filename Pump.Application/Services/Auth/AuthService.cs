using Microsoft.EntityFrameworkCore;
using Pump.Application.Common;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Auth;
using Pump.Application.Interfaces.Auth;
using Pump.Application.Interfaces.Common;
using Pump.Domain.Entities;
using Pump.Domain.Enums;
using System.Security.Cryptography;

namespace Pump.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;
    private readonly IEmailSender _emailSender;

    public AuthService(IApplicationDbContext db, IPasswordHasher passwordHasher, IJwtTokenService jwt , IEmailSender emailSender)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _emailSender = emailSender;
    }


    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default)
    {
        var usernameLower = request.Username.ToLowerInvariant();
        var emailLower = request.Email.ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(
            u => u.Username == usernameLower || u.Email == emailLower, ct);
        if (exists)
            throw new ConflictException("Username or email is already taken.");

        var user = new User
        {
            Email = emailLower,
            Username = usernameLower,
            FullName = request.FullName,
            Role = UserRole.User
        };
        user.PasswordHash = _passwordHasher.Hash(request.Password);

        _db.Users.Add(user);

        return await IssueTokensAsync(user, ct);
    }



    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        var key = request.UsernameOrEmail.ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(
            u => u.Username == key || u.Email == key, ct);

        if (user is null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new UnauthorizedException("Invalid credentials.");

        return await IssueTokensAsync(user, ct);
    }



    public async Task<AuthResponseDto> RefreshAsync(RefreshRequestDto request, CancellationToken ct = default)
    {
        var hashed = _jwt.HashToken(request.RefreshToken);

        var existing = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == hashed, ct);

        if (existing is null || existing.RevokedAtUtc is not null || existing.ExpiresAtUtc < DateTime.UtcNow)
            throw new UnauthorizedException("Invalid or expired refresh token.");


        existing.RevokedAtUtc = DateTime.UtcNow;

        var (accessToken, expiresIn) = _jwt.GenerateAccessToken(existing.User);
        var newRawToken = _jwt.GenerateRefreshToken();
        var newHash = _jwt.HashToken(newRawToken);

        existing.ReplacedByTokenHash = newHash;

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = existing.UserId,
            TokenHash = newHash,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(14)
        });

        await _db.SaveChangesAsync(ct);

        return new AuthResponseDto(existing.UserId, accessToken, newRawToken, expiresIn);
    }

    public async Task LogoutAsync(int userId, LogoutRequestDto request, CancellationToken ct = default)
    {
        var hashed = _jwt.HashToken(request.RefreshToken);

        var existing = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hashed, ct);

        if (existing is not null && existing.UserId == userId && existing.RevokedAtUtc is null)
        {
            existing.RevokedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }


    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken ct = default)
    {
        var email = request.Email.ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null) return;

        // cancel any previous codes that are still valid (not used and not expired)
        var oldCodes = await _db.PasswordResetCodes
            .Where(c => c.UserId == user.Id && !c.IsUsed)
            .ToListAsync(ct);
        foreach (var old in oldCodes.Where(IsStillValid))
            old.IsUsed = true;

        var rawCode = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        _db.PasswordResetCodes.Add(new PasswordResetCode
        {
            UserId = user.Id,
            CodeHash = _jwt.HashToken(rawCode)
        });

        await _db.SaveChangesAsync(ct);

        await _emailSender.SendAsync(
            user.Email,
            "PUMP — Password Reset Code",
            $"<p>Your password reset code is:</p><h2>{rawCode}</h2><p>This code expires in {AuthConstants.ResetCodeExpiryMinutes} minutes.</p>",
            ct);
    }

    public async Task<VerifyResetCodeResponseDto> VerifyResetCodeAsync(VerifyResetCodeRequestDto request, CancellationToken ct = default)
    {
        var email = request.Email.ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct)
            ?? throw new UnauthorizedException("Invalid or expired code.");

        var codeHash = _jwt.HashToken(request.Code);

        var resetCode = await _db.PasswordResetCodes
            .Where(c => c.UserId == user.Id && c.CodeHash == codeHash && !c.IsUsed)
            .OrderByDescending(c => c.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (resetCode is null || !IsStillValid(resetCode))
            throw new UnauthorizedException("Invalid or expired code.");

        var rawResetToken = _jwt.GenerateRefreshToken();
        resetCode.ResetTokenHash = _jwt.HashToken(rawResetToken);
        resetCode.VerifiedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new VerifyResetCodeResponseDto(rawResetToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken ct = default)
    {
        var tokenHash = _jwt.HashToken(request.ResetToken);

        var resetCode = await _db.PasswordResetCodes
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.ResetTokenHash == tokenHash && !c.IsUsed, ct);

        if (resetCode is null || resetCode.VerifiedAtUtc is null || IsResetTokenExpired(resetCode))
            throw new UnauthorizedException("Invalid or expired reset token.");

        resetCode.IsUsed = true;
        resetCode.User.PasswordHash = _passwordHasher.Hash(request.NewPassword);

        var activeTokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == resetCode.UserId && rt.RevokedAtUtc == null)
            .ToListAsync(ct);
        foreach (var t in activeTokens) t.RevokedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }


    private static bool IsStillValid(PasswordResetCode code)
        => !code.IsUsed && code.CreatedAtUtc.AddMinutes(AuthConstants.ResetCodeExpiryMinutes) > DateTime.UtcNow;


    private static bool IsResetTokenExpired(PasswordResetCode code)
        => code.VerifiedAtUtc!.Value.AddMinutes(AuthConstants.ResetTokenExpiryMinutes) < DateTime.UtcNow;



private async Task<AuthResponseDto> IssueTokensAsync(User user, CancellationToken ct)
    {
        var (accessToken, expiresIn) = _jwt.GenerateAccessToken(user);
        var rawRefreshToken = _jwt.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            User = user,  
            TokenHash = _jwt.HashToken(rawRefreshToken),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(14)
        });

        await _db.SaveChangesAsync(ct);

        return new AuthResponseDto(user.Id, accessToken, rawRefreshToken, expiresIn);
    }
}
