namespace Pump.Application.Dtos.Auth
{
    public record VerifyResetCodeRequestDto(string Email, string Code);
}
