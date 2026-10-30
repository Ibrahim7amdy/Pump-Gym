using Pump.Domain.Entities;

namespace Pump.Application.Interfaces.Auth
{
    public interface IJwtTokenService
    {
        (string token, int expiresInSeconds) GenerateAccessToken(User user);
        string GenerateRefreshToken();          // raw token given to client
        string HashToken(string rawToken);     // stored in DB
    }
}
