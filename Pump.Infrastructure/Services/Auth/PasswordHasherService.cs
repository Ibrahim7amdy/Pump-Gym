using Microsoft.AspNetCore.Identity;
using Pump.Application.Interfaces.Auth;
using Pump.Domain.Entities;

namespace Pump.Infrastructure.Services.Auth;

public class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(null!, password);

    public bool Verify(string hashedPassword, string providedPassword)
        => _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword)
            != PasswordVerificationResult.Failed;
}