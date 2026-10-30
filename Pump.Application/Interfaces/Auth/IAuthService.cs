using Pump.Application.Dtos.Auth;


namespace Pump.Application.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
        Task<AuthResponseDto> RefreshAsync(RefreshRequestDto request, CancellationToken ct = default);
        Task LogoutAsync(int userId, LogoutRequestDto request, CancellationToken ct = default);
        Task ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken ct = default);
        Task<VerifyResetCodeResponseDto> VerifyResetCodeAsync(VerifyResetCodeRequestDto request, CancellationToken ct = default);
        Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken ct = default);
    }
}
