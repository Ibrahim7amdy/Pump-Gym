using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pump.Application.Interfaces.Auth;
using Pump.Application.Interfaces.Common;
using Pump.Application.Interfaces.Users;
using Pump.Application.Interfaces.Visits;
using Pump.Infrastructure.Jobs;
using Pump.Infrastructure.Persistence;
using Pump.Infrastructure.Persistence.Seed;
using Pump.Infrastructure.Services.Auth;
using Pump.Infrastructure.Services.Common;
using Pump.Infrastructure.Services.Storage;
using Pump.Infrastructure.Services.Users;
using Pump.Infrastructure.Services.Visits;


namespace Pump.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("Default")));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IPasswordHasher, PasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IAvatarPresetProvider, AvatarPresetProvider>();
        services.AddSingleton<IGymQrSettings, GymQrSettings>();

        services.AddScoped<IEmailSender, SendGridEmailSender>();

        services.AddScoped<StaleSessionCleanupJob>();

        services.AddScoped<DbSeeder>();

        return services;
    }
}
