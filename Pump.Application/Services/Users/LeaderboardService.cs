using Microsoft.EntityFrameworkCore;
using Pump.Application.Common;
using Pump.Application.Dtos.Users;
using Pump.Application.Interfaces.Common;
using Pump.Application.Interfaces.Users;
using Pump.Domain.Enums;

namespace Pump.Application.Services.Users;

public sealed class LeaderboardService : ILeaderboardService
{
    private readonly IApplicationDbContext _db;

    public LeaderboardService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<LeaderboardItemDto>> GetAsync(
        string? tier,
        string timeframe,
        CancellationToken ct = default)
    {
        var normalizedTimeframe = timeframe.Trim();
        var now = DateTime.UtcNow;
        var periodStart = normalizedTimeframe switch
        {
            "week" => StartOfUtcWeek(now),
            "month" => new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => (DateTime?)null
        };
        var periodEnd = normalizedTimeframe switch
        {
            "week" => periodStart!.Value.AddDays(7),
            "month" => periodStart!.Value.AddMonths(1),
            _ => (DateTime?)null
        };

        var athletes = await _db.Users
            .AsNoTracking()
            .Where(user => user.Role == UserRole.User && user.IsVerified)
            .Select(user => new
            {
                user.Id,
                user.Username,
                user.FullName,
                user.AvatarUrl,
                user.Xp
            })
            .ToListAsync(ct);

        Dictionary<int, int>? pointsByUserId = null;
        if (periodStart.HasValue && periodEnd.HasValue)
        {
            pointsByUserId = await _db.Sessions
                .AsNoTracking()
                .Where(session => session.User.Role == UserRole.User
                    && session.User.IsVerified
                    && session.EndedAtUtc != null
                    && session.StartedAtUtc >= periodStart.Value
                    && session.StartedAtUtc < periodEnd.Value)
                .GroupBy(session => session.UserId)
                .Select(group => new
                {
                    UserId = group.Key,
                    Points = group.Sum(session => session.XpAwarded ?? 0)
                })
                .ToDictionaryAsync(item => item.UserId, item => item.Points, ct);
        }

        var ranked = athletes
            .Select(athlete => new
            {
                athlete.Id,
                athlete.Username,
                athlete.FullName,
                athlete.AvatarUrl,
                Tier = AthleteProgressionCalculator.GetTier(athlete.Xp),
                Points = pointsByUserId is null
                    ? athlete.Xp
                    : pointsByUserId.GetValueOrDefault(athlete.Id)
            })
            .Where(athlete => string.IsNullOrWhiteSpace(tier)
                || string.Equals(tier, "All", StringComparison.OrdinalIgnoreCase)
                || string.Equals(athlete.Tier, tier, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(athlete => athlete.Points)
            .ThenBy(athlete => athlete.FullName)
            .ThenBy(athlete => athlete.Id)
            .ToList();

        return ranked.Select((athlete, index) => new LeaderboardItemDto(
                index + 1,
                athlete.Id,
                athlete.FullName,
                athlete.Username,
                athlete.AvatarUrl,
                athlete.Tier,
                athlete.Points,
                true))
            .ToList();
    }

    private static DateTime StartOfUtcWeek(DateTime utcNow)
    {
        var daysSinceMonday = ((int)utcNow.DayOfWeek + 6) % 7;
        return new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, DateTimeKind.Utc)
            .AddDays(-daysSinceMonday);
    }
}
