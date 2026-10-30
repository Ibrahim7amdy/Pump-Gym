using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Pump.Application.Interfaces.Auth;
using Pump.Domain.Entities;
using Pump.Domain.Enums;

namespace Pump.Infrastructure.Persistence.Seed;

public class DbSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _config;

    public DbSeeder(ApplicationDbContext db, IPasswordHasher passwordHasher, IConfiguration config)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _config = config;
    }

    public async Task SeedAsync()
    {
        if (await _db.Users.AnyAsync(u => u.Role == UserRole.Admin))
            return;

        var adminEmail = _config["Seed:AdminEmail"] ?? "admin@pumpgym.app";
        var adminPassword = _config["Seed:AdminPassword"] ?? "Admin@12345!";

        var admin = new User
        {
            Email = adminEmail,
            Username = "super_admin",
            FullName = "Super Admin",
            Role = UserRole.Admin,
            PasswordHash = _passwordHasher.Hash(adminPassword)
        };

        _db.Users.Add(admin);
        await _db.SaveChangesAsync();
    }
}