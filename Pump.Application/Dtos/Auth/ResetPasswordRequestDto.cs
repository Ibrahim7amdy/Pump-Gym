
namespace Pump.Application.Dtos.Auth
{
    public record ResetPasswordRequestDto(string ResetToken, string NewPassword);
}
