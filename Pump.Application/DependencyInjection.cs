using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Pump.Application.Interfaces.Auth;
using Pump.Application.Interfaces.Analytics;
using Pump.Application.Interfaces.Home;
using Pump.Application.Interfaces.Machines;
using Pump.Application.Interfaces.Protocols;
using Pump.Application.Interfaces.Sessions;
using Pump.Application.Interfaces.Squads;
using Pump.Application.Interfaces.Users;
using Pump.Application.Interfaces.Visits;
using Pump.Application.Services.Auth;
using Pump.Application.Services.Analytics;
using Pump.Application.Services.Home;
using Pump.Application.Services.Machines;
using Pump.Application.Services.Protocols;
using Pump.Application.Services.Sessions;
using Pump.Application.Services.Squads;
using Pump.Application.Services.Users;
using Pump.Application.Services.Visits;


namespace Pump.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<ILeaderboardService, LeaderboardService>();
        services.AddScoped<IAdminAnalyticsService, AdminAnalyticsService>();
        services.AddScoped<IDropInPricingService, DropInPricingService>();
        services.AddScoped<IHomeDashboardService, HomeDashboardService>();
        services.AddScoped<IAdminAthleteService, AdminAthleteService>();
        services.AddScoped<IProtocolService, ProtocolService>();
        services.AddScoped<IMachineService, MachineService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IVisitService, VisitService>();
        services.AddScoped<ISquadService, SquadService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
