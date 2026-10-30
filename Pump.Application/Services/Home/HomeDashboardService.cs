using Microsoft.EntityFrameworkCore;
using Pump.Application.Common;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Home;
using Pump.Application.Interfaces.Common;
using Pump.Application.Interfaces.Home;
using Pump.Application.Interfaces.Protocols;
using Pump.Domain.Enums;

namespace Pump.Application.Services.Home;

public sealed class HomeDashboardService : IHomeDashboardService
{
    private readonly IApplicationDbContext _db;
    private readonly IProtocolService _protocolService;

    public HomeDashboardService(IApplicationDbContext db, IProtocolService protocolService)
    {
        _db = db;
        _protocolService = protocolService;
    }

    public async Task<HomeDashboardDto> GetAsync(int userId, CancellationToken ct = default)
    {
        var athlete = await _db.Users
            .AsNoTracking()
            .Where(user => user.Id == userId && user.Role == UserRole.User)
            .Select(user => new
            {
                user.Id,
                user.FullName,
                user.Username,
                user.AvatarUrl,
                user.PreferredLanguage,
                user.PreferredThemeId,
                user.Xp
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("Athlete not found.");

        var isCheckedIn = await _db.Visits
            .AnyAsync(visit => visit.UserId == userId && visit.CheckOutAtUtc == null, ct);

        var athletesInGymCount = await _db.Visits
            .CountAsync(visit => visit.UserId != null
                && visit.CheckOutAtUtc == null
                && visit.User!.Role == UserRole.User, ct);

        var activeMachinesCount = await _db.Machines
            .CountAsync(machine => machine.IsActive, ct);

        var unreadNotificationsCount = await _db.Notifications
            .CountAsync(notification => notification.UserId == userId && !notification.IsRead, ct);

        HomeActiveProtocolDto? activeProtocol = null;
        try
        {
            var protocol = await _protocolService.GetActiveAsync(userId, ct);
            var imageUrl = await _db.Protocols
                .AsNoTracking()
                .Where(item => item.Id == protocol.ProtocolId)
                .Select(item => item.ImageUrl)
                .FirstOrDefaultAsync(ct);

            activeProtocol = new HomeActiveProtocolDto(
                protocol.ProtocolId,
                protocol.AssignmentId,
                protocol.Title,
                imageUrl,
                protocol.CoachName,
                protocol.Status,
                protocol.IsOpenProtocol,
                protocol.CurrentWeek,
                protocol.TotalWeeks,
                protocol.ProgressPercent,
                protocol.CompletedDaysCount,
                protocol.TodayProtocolDay,
                protocol.ExpectedGainLabel,
                protocol.ExpectedMuscleGainLabel,
                protocol.ExpectedStrengthGainLabel,
                protocol.ExpectedPoints,
                protocol.EstimatedCaloriesBurned,
                protocol.PlannedVolumeKg,
                protocol.PlannedVolumeByMuscleGroup,
                protocol.WeeklyLoadProgression);
        }
        catch (NotFoundException)
        {
            // A new athlete can use the home page before an admin assigns a protocol.
        }

        var currentSession = await _db.Sessions
            .AsNoTracking()
            .Where(session => session.UserId == userId && session.EndedAtUtc == null)
            .Select(session => new HomeCurrentSessionDto(
                session.Id,
                session.StartedAtUtc,
                session.ProtocolDayId,
                session.ProtocolDay == null ? null : session.ProtocolDay.Label,
                session.Stations.Sum(station => station.Sets.Count),
                session.Stations
                    .OrderBy(station => station.SortOrder)
                    .Select(station => new HomeCurrentSessionStationDto(
                        station.Id,
                        station.MachineId,
                        station.Machine.Name,
                        station.TargetSets,
                        station.Sets.Count))
                    .ToList()))
            .FirstOrDefaultAsync(ct);

        var (level, levelProgressPercent) = AthleteProgressionCalculator.GetLevel(athlete.Xp);

        return new HomeDashboardDto(
            athlete.Id,
            athlete.FullName,
            athlete.Username,
            athlete.AvatarUrl,
            athlete.PreferredLanguage,
            athlete.PreferredThemeId.ToString(),
            athlete.Xp,
            AthleteProgressionCalculator.GetTier(athlete.Xp),
            level,
            levelProgressPercent,
            isCheckedIn,
            athletesInGymCount,
            activeMachinesCount,
            unreadNotificationsCount,
            activeProtocol,
            currentSession);
    }
}
