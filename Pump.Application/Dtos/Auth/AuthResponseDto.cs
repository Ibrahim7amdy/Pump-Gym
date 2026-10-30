namespace Pump.Application.Dtos.Auth
{
    public record AuthResponseDto(int UserId, string AccessToken, string RefreshToken, int ExpiresInSeconds);
}
