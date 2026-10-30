namespace Pump.Application.Dtos.Auth
{
    public record RegisterRequestDto(string Email, string Username, string Password, string FullName);
}
