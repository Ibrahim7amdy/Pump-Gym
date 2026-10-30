using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pump.Application.Common.Exceptions;
using Pump.Application.Common;
using Pump.Application.Dtos.Sessions;
using Pump.Application.Interfaces.Common;
using Pump.Application.Interfaces.Sessions;
using Pump.Domain.Entities;
using Pump.Domain.Enums;


namespace Pump.Application.Services.Sessions;
public class SessionService : ISessionService
{
    private const double MetValueForResistanceTraining = 6.0;
    private const int StaleSessionThresholdHours = 6;

    private readonly IApplicationDbContext _db;
    private readonly ILogger<SessionService> _logger;

    public SessionService(IApplicationDbContext db, ILogger<SessionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<SessionDto> StartAsync(int userId, CreateSessionRequestDto request, CancellationToken ct = default)
    {
        var hasOpenSession = await _db.Sessions.AnyAsync(s => s.UserId == userId && s.EndedAtUtc == null, ct);
        if (hasOpenSession)
            throw new ConflictException("You already have an active session. Finish it before starting a new one.");

        var isCheckedIn = await _db.Visits
            .AnyAsync(visit => visit.UserId == userId && visit.CheckOutAtUtc == null, ct);
        if (!isCheckedIn)
            throw new ConflictException("Check in at the gym before starting a workout.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeAssignment = await _db.AthleteProtocolAssignments
            .FirstOrDefaultAsync(a => a.UserId == userId
                && a.Status == AssignmentStatus.Active
                && a.ExpiresOnUtc >= today, ct);

        var session = new Session
        {
            UserId = userId,
            AthleteProtocolAssignmentId = activeAssignment?.Id,
            StartedAtUtc = DateTime.UtcNow
        };

        if (request.ProtocolDayId is int protocolDayId)
        {
            var protocolDay = await _db.ProtocolDays
                .Include(d => d.Exercises).ThenInclude(e => e.Machine)
                .FirstOrDefaultAsync(d => d.Id == protocolDayId, ct)
                ?? throw new NotFoundException("Protocol day not found.");

            if (activeAssignment is null || protocolDay.ProtocolId != activeAssignment.ProtocolId)
                throw new ConflictException("This protocol day does not belong to your active protocol.");

            session.ProtocolDayId = protocolDayId;
            session.Stations = protocolDay.Exercises
                .OrderBy(e => e.SortOrder)
                .Select(e => new SessionStation
                {
                    MachineId = e.MachineId,
                    ProtocolExerciseId = e.Id,
                    TargetSets = e.Sets,
                    RestSeconds = e.RestSeconds,
                    SortOrder = e.SortOrder
                })
                .ToList();
        }

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(session.Id, userId, ct);
    }

    public async Task<SessionDto> GetByIdAsync(int sessionId, int userId, CancellationToken ct = default)
    {
        var session = await _db.Sessions
            .Include(s => s.User)
            .Include(s => s.ProtocolDay)
            .Include(s => s.Stations).ThenInclude(st => st.Machine)
            .Include(s => s.Stations).ThenInclude(st => st.Sets)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, ct)
            ?? throw new NotFoundException("Session not found.");

        return ToSessionDto(session);
    }

    public async Task<SessionDto> GetCurrentAsync(int userId, CancellationToken ct = default)
    {
        var session = await _db.Sessions
            .Include(s => s.User)
            .Include(s => s.ProtocolDay)
            .Include(s => s.Stations).ThenInclude(st => st.Machine)
            .Include(s => s.Stations).ThenInclude(st => st.Sets)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.EndedAtUtc == null, ct)
            ?? throw new NotFoundException("No active session found.");

        return ToSessionDto(session);
    }

    public async Task<WorkoutHistoryDto> GetHistoryAsync(int userId, int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1 || pageSize is < 1 or > 100)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["pagination"] = ["Page must be at least 1 and pageSize must be between 1 and 100."]
            });
        }

        var query = _db.Sessions
            .AsNoTracking()
            .Where(s => s.UserId == userId
                && s.EndedAtUtc != null
                && s.Stations.Any(station => station.Sets.Any()));

        var totalCount = await query.CountAsync(ct);
        var skip = (int)Math.Min((long)(page - 1) * pageSize, int.MaxValue);
        var sessions = await query
            .OrderByDescending(s => s.StartedAtUtc)
            .Include(s => s.ProtocolDay)
            .Include(s => s.AthleteProtocolAssignment).ThenInclude(a => a!.Protocol)
            .Include(s => s.Stations).ThenInclude(st => st.Machine)
            .Include(s => s.Stations).ThenInclude(st => st.Sets)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        var allSessionsForStats = await query
            .Select(session => new { session.StartedAtUtc, session.TotalVolumeKg, session.IntensityPercent })
            .ToListAsync(ct);
        var intensities = allSessionsForStats
            .Where(session => session.IntensityPercent.HasValue)
            .Select(session => session.IntensityPercent!.Value)
            .ToList();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var firstActivityDay = today.AddDays(-29);
        var activityStartUtc = firstActivityDay.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var activityEndUtc = today.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var recentSessions = await query
            .Where(session => session.StartedAtUtc >= activityStartUtc && session.StartedAtUtc < activityEndUtc)
            .Include(session => session.ProtocolDay)
            .Include(session => session.AthleteProtocolAssignment).ThenInclude(assignment => assignment!.Protocol)
            .Include(session => session.Stations).ThenInclude(station => station.Machine)
            .Include(session => session.Stations).ThenInclude(station => station.Sets)
            .OrderBy(session => session.StartedAtUtc)
            .ToListAsync(ct);

        var sessionsByDate = recentSessions
            .GroupBy(session => DateOnly.FromDateTime(session.StartedAtUtc))
            .ToDictionary(group => group.Key, group => group.ToList());
        var activityDays = new List<SessionHistoryActivityDayDto>(30);
        var restDaysCount = 0;
        for (var index = 0; index < 30; index++)
        {
            var date = firstActivityDay.AddDays(index);
            sessionsByDate.TryGetValue(date, out var daySessions);
            daySessions ??= [];

            var dayIntensities = daySessions
                .Where(session => session.IntensityPercent.HasValue)
                .Select(session => session.IntensityPercent!.Value)
                .ToList();
            var averageIntensity = dayIntensities.Count == 0 ? (double?)null : dayIntensities.Average();
            var intensityLevel = GetHistoryIntensityLevel(daySessions.Count, averageIntensity);
            if (daySessions.Count == 0)
                restDaysCount++;

            activityDays.Add(new SessionHistoryActivityDayDto(
                date,
                index + 1,
                daySessions.Count,
                daySessions.Sum(session => session.TotalVolumeKg ?? 0),
                averageIntensity,
                intensityLevel,
                daySessions.OrderByDescending(session => session.StartedAtUtc).Select(ToHistoryItem).ToList()));
        }

        var recordHistory = await GetPersonalRecordHistoryForLatestAssignmentAsync(userId, ct);
        var items = sessions.Select(ToHistoryItem).ToList();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);

        return new WorkoutHistoryDto(
            totalCount,
            allSessionsForStats.Sum(session => session.TotalVolumeKg ?? 0),
            intensities.Count == 0 ? null : intensities.Average(),
            GetLongestTrainingStreak(allSessionsForStats.Select(session => session.StartedAtUtc)),
            restDaysCount,
            recordHistory,
            activityDays,
            items,
            page,
            pageSize,
            totalCount,
            totalPages);
    }

    private async Task<ProtocolPersonalRecordHistoryDto> GetPersonalRecordHistoryForLatestAssignmentAsync(int userId, CancellationToken ct)
    {
        var latestAssignment = await _db.AthleteProtocolAssignments
            .Where(assignment => assignment.UserId == userId)
            .OrderByDescending(assignment => assignment.AssignedOnUtc)
            .ThenByDescending(assignment => assignment.Id)
            .Select(assignment => new { assignment.Id, assignment.ProtocolId })
            .FirstOrDefaultAsync(ct);

        if (latestAssignment is null)
            return new ProtocolPersonalRecordHistoryDto(null, null, null, 0, []);

        var protocolTitle = await _db.Protocols
            .IgnoreQueryFilters()
            .Where(protocol => protocol.Id == latestAssignment.ProtocolId)
            .Select(protocol => protocol.Title)
            .FirstOrDefaultAsync(ct);

        var sets = await _db.SessionSets
            .AsNoTracking()
            .Where(set => set.SessionStation.Session.UserId == userId
                && set.SessionStation.Session.EndedAtUtc != null)
            .Select(set => new
            {
                SessionId = set.SessionStation.SessionId,
                AssignmentId = set.SessionStation.Session.AthleteProtocolAssignmentId,
                StartedAtUtc = set.SessionStation.Session.StartedAtUtc,
                ProtocolDayLabel = set.SessionStation.Session.ProtocolDay != null
                    ? set.SessionStation.Session.ProtocolDay.Label
                    : null,
                MachineId = set.SessionStation.MachineId,
                MachineName = set.SessionStation.Machine.Name,
                MuscleGroup = set.SessionStation.Machine.MuscleGroup,
                set.WeightKg,
                set.Reps,
                set.LoggedAtUtc
            })
            .OrderBy(set => set.StartedAtUtc)
            .ThenBy(set => set.LoggedAtUtc)
            .ToListAsync(ct);

        var bestWeightByMachine = new Dictionary<int, decimal>();
        var records = new List<PersonalRecordAchievementDto>();

        var sessionGroups = sets
            .GroupBy(set => new
            {
                set.SessionId,
                set.AssignmentId,
                set.StartedAtUtc,
                set.ProtocolDayLabel
            })
            .OrderBy(group => group.Key.StartedAtUtc)
            .ThenBy(group => group.Key.SessionId);

        foreach (var session in sessionGroups)
        {
            foreach (var machineSession in session.GroupBy(set => new
                {
                    set.MachineId,
                    set.MachineName,
                    set.MuscleGroup
                }))
            {
                var bestSet = machineSession
                    .OrderByDescending(set => set.WeightKg)
                    .ThenBy(set => set.LoggedAtUtc)
                    .First();
                var hasPreviousRecord = bestWeightByMachine.TryGetValue(machineSession.Key.MachineId, out var previousRecordKg);

                if (session.Key.AssignmentId == latestAssignment.Id
                    && (!hasPreviousRecord || bestSet.WeightKg > previousRecordKg))
                {
                    records.Add(new PersonalRecordAchievementDto(
                        session.Key.SessionId,
                        machineSession.Key.MachineId,
                        machineSession.Key.MachineName,
                        machineSession.Key.MuscleGroup,
                        session.Key.ProtocolDayLabel,
                        hasPreviousRecord ? previousRecordKg : null,
                        bestSet.WeightKg,
                        bestSet.Reps,
                        bestSet.LoggedAtUtc));
                }

                if (!hasPreviousRecord || bestSet.WeightKg > previousRecordKg)
                    bestWeightByMachine[machineSession.Key.MachineId] = bestSet.WeightKg;
            }
        }

        var orderedRecords = records
            .OrderBy(record => record.AchievedAtUtc)
            .ThenBy(record => record.MachineId)
            .ToList();

        return new ProtocolPersonalRecordHistoryDto(
            latestAssignment.Id,
            latestAssignment.ProtocolId,
            protocolTitle,
            orderedRecords.Count,
            orderedRecords);
    }

    public async Task<SessionSetDto> LogSetAsync(int sessionId, int userId, LogSetRequestDto request, CancellationToken ct = default)
    {
        var session = await _db.Sessions
            .Include(s => s.Stations).ThenInclude(st => st.Sets)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, ct)
            ?? throw new NotFoundException("Session not found.");

        if (session.EndedAtUtc is not null)
            throw new ConflictException("This session has already ended.");

        var station = session.Stations.FirstOrDefault(st => st.Id == request.SessionStationId)
            ?? throw new NotFoundException("Station not found in this session.");

        var set = new SessionSet
        {
            SessionStationId = station.Id,
            SetNumber = station.Sets.Count + 1,
            WeightKg = request.WeightKg,
            Reps = request.Reps,
            LoggedAtUtc = DateTime.UtcNow
        };

        _db.SessionSets.Add(set);
        await _db.SaveChangesAsync(ct);

        return new SessionSetDto(set.Id, set.SetNumber, set.WeightKg, set.Reps, set.LoggedAtUtc);
    }

    public async Task<SessionStationDto> AddStationAsync(int sessionId, int userId, AddStationRequestDto request, CancellationToken ct = default)
    {
        var session = await _db.Sessions
            .Include(s => s.Stations)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, ct)
            ?? throw new NotFoundException("Session not found.");

        if (session.EndedAtUtc is not null)
            throw new ConflictException("This session has already ended.");

        var machine = await _db.Machines.FirstOrDefaultAsync(m => m.Id == request.MachineId && m.IsActive, ct)
            ?? throw new NotFoundException("Machine not found or inactive.");

        var station = new SessionStation
        {
            SessionId = sessionId,
            MachineId = request.MachineId,
            ProtocolExerciseId = null,
            TargetSets = request.TargetSets,
            RestSeconds = request.RestSeconds,
            SortOrder = session.Stations.Count > 0 ? session.Stations.Max(s => s.SortOrder) + 1 : 0
        };

        _db.SessionStations.Add(station);
        await _db.SaveChangesAsync(ct);

        return ToStationDto(station, machine);
    }

    public async Task<SessionStationDto> ReplaceStationMachineAsync(int sessionId, int stationId, int userId, ReplaceStationMachineRequestDto request, CancellationToken ct = default)
    {
        var session = await _db.Sessions
            .Include(s => s.Stations).ThenInclude(st => st.Sets)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, ct)
            ?? throw new NotFoundException("Session not found.");

        if (session.EndedAtUtc is not null)
            throw new ConflictException("This session has already ended.");

        var station = session.Stations.FirstOrDefault(st => st.Id == stationId)
            ?? throw new NotFoundException("Station not found in this session.");

        var newMachine = await _db.Machines.FirstOrDefaultAsync(m => m.Id == request.NewMachineId && m.IsActive, ct)
            ?? throw new NotFoundException("Machine not found or inactive.");

        // prevent replacing the machine if any sets have already been logged for this station
        if (station.Sets.Count > 0)
            throw new ConflictException("Cannot replace machine after sets have been logged for this station.");

        station.MachineId = request.NewMachineId;
        station.ProtocolExerciseId = null;   // بقت مستقلة عن خطة اليوم الأصلية بعد الاستبدال

        await _db.SaveChangesAsync(ct);

        return ToStationDto(station, newMachine);
    }

    public async Task<SessionDebriefDto> FinishAsync(int sessionId, int userId, CancellationToken ct = default)
    {
        var session = await _db.Sessions
            .Include(s => s.User)
            .Include(s => s.Stations).ThenInclude(st => st.Sets)
            .Include(s => s.Stations).ThenInclude(st => st.ProtocolExercise)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, ct)
            ?? throw new NotFoundException("Session not found.");

        if (session.EndedAtUtc is not null)
            throw new ConflictException("This session has already ended.");

        try
        {
            return await FinishSessionCoreAsync(session, DateTime.UtcNow, ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            _db.ChangeTracker.Clear();
            throw new ConflictException("This session was already completed or changed by another request.");
        }
    }

    public async Task<int> CloseStaleSessionsAsync(
       CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddHours(-StaleSessionThresholdHours);

        var staleSessionIds = await _db.Sessions
            .AsNoTracking()
            .Where(s =>
                s.EndedAtUtc == null &&
                s.StartedAtUtc < cutoff)
            .Select(s => s.Id)
            .ToListAsync(ct);

        var closedCount = 0;

        foreach (var sessionId in staleSessionIds)
        {
            try
            {
                // Load a fresh session for each iteration.
                var session = await _db.Sessions
                    .Include(s => s.User)
                    .Include(s => s.Stations).ThenInclude(st => st.Sets)
                    .Include(s => s.Stations).ThenInclude(st => st.ProtocolExercise)
                    .FirstOrDefaultAsync(s => s.Id == sessionId && s.EndedAtUtc == null && s.StartedAtUtc < cutoff, ct);

                // It may have been completed manually since we fetched the IDs.
                if (session is null)
                {
                    _db.ChangeTracker.Clear();
                    continue;
                }

                var lastActivity = session.Stations
                    .SelectMany(st => st.Sets)
                    .Select(set => set.LoggedAtUtc)
                    .DefaultIfEmpty(session.StartedAtUtc)
                    .Max();

                // Use the same finalization logic as manual completion.
                await FinishSessionCoreAsync(session, lastActivity, ct);

                closedCount++;

                _logger.LogInformation("Stale session {SessionId} was automatically completed.", sessionId);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _db.ChangeTracker.Clear();
                throw;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex,"Stale session {SessionId} was modified by another operation. Skipping it.",sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Failed to automatically complete stale session {SessionId}. Continuing with the remaining sessions.", sessionId);
            }
            finally
            {
                // Prevent failed changes from being persisted with a later session.
                _db.ChangeTracker.Clear();
            }
        }

        return closedCount;
    }

    private async Task<SessionDebriefDto> FinishSessionCoreAsync(Session session, DateTime endedAt, CancellationToken ct)
    {
        session.EndedAtUtc = endedAt;

        var allSets = session.Stations.SelectMany(st => st.Sets).ToList();

        var totalVolumeKg = allSets.Sum(s => s.WeightKg * s.Reps);
        session.TotalVolumeKg = totalVolumeKg;

        var durationHours = (endedAt - session.StartedAtUtc).TotalHours;
        var userWeightKg = session.User.CurrentWeightKg;
        int? caloriesEstimate = userWeightKg is null
                    ? null
                    : (int)Math.Round(MetValueForResistanceTraining * (double)userWeightKg.Value * durationHours);
        session.CaloriesEstimate = caloriesEstimate;

        double? intensityPercent = null;
        var stationsWithTarget = session.Stations.Where(st => st.ProtocolExercise is not null).ToList();

        if (stationsWithTarget.Count > 0)
        {
            var actualWeightSum = stationsWithTarget.SelectMany(st => st.Sets).Sum(s => s.WeightKg);
            var targetWeightSum = stationsWithTarget.Sum(st => st.ProtocolExercise!.TargetWeightKg * st.TargetSets);
            intensityPercent = targetWeightSum == 0 ? 0 : Math.Round((double)(actualWeightSum / targetWeightSum) * 100, 0);
        }
        else if (session.Stations.Count > 0)
        {
            var completedSets = allSets.Count;
            var targetSets = session.Stations.Sum(st => st.TargetSets);
            intensityPercent = targetSets == 0 ? 0 : Math.Round((double)completedSets / targetSets * 100, 0);
        }
        session.IntensityPercent = intensityPercent;

        var completedSetsCount = allSets.Count;
        var isFullyCompleted = session.Stations.Count > 0 && session.Stations.All(st => st.Sets.Count >= st.TargetSets);

        var xpAwarded = (int)Math.Round(
            ((double)totalVolumeKg * 0.5) +
            (completedSetsCount * 5) +
            (isFullyCompleted ? 50 : 0));
        session.XpAwarded = xpAwarded;

        session.User.Xp += xpAwarded;
        var (level, levelProgressPercent) = AthleteProgressionCalculator.GetLevel(session.User.Xp);

        if (allSets.Count > 0 && session.AthleteProtocolAssignmentId is int assignmentId)
        {
            var assignment = await _db.AthleteProtocolAssignments.FirstOrDefaultAsync(a => a.Id == assignmentId, ct);
            if (assignment is not null)
            {
                // Both open and scheduled protocols count distinct completed sessions
                // containing sets. The current session is not persisted as ended yet.
                var completedSessionIds = await _db.Sessions
                    .Where(s => s.AthleteProtocolAssignmentId == assignmentId
                        && s.EndedAtUtc != null
                        && s.Stations.Any(station => station.Sets.Any())
                        && s.Id != session.Id)
                    .Select(s => s.Id)
                    .Distinct()
                    .ToListAsync(ct);
                assignment.CompletedDaysCount = completedSessionIds.Count + 1;

                if (assignment.Status == AssignmentStatus.Active)
                {
                    var today = DateOnly.FromDateTime(DateTime.UtcNow);
                    var workoutDayCount = await _db.ProtocolDays
                        .CountAsync(day => day.ProtocolId == assignment.ProtocolId
                            && day.DayNumber.HasValue
                            && day.Intensity != ProtocolDayIntensity.Rest, ct);
                    if (workoutDayCount > 0 && assignment.CompletedDaysCount >= workoutDayCount)
                        assignment.Status = AssignmentStatus.Completed;
                    else if (assignment.ExpiresOnUtc < today)
                        assignment.Status = AssignmentStatus.Expired;
                }
            }
        }

        await _db.SaveChangesAsync(ct);

        return new SessionDebriefDto(
            session.Id, xpAwarded, level, levelProgressPercent,
            intensityPercent ?? 0, FormatDuration(durationHours),
            totalVolumeKg, caloriesEstimate);
    }

    private static string FormatDuration(double hours)
    {
        var ts = TimeSpan.FromHours(hours);
        return $"{(int)ts.TotalHours}:{ts.Minutes:D2}";
    }

    private static int GetLongestTrainingStreak(IEnumerable<DateTime> workoutDatesUtc)
    {
        var dates = workoutDatesUtc
            .Select(DateOnly.FromDateTime)
            .Distinct()
            .OrderBy(date => date)
            .ToList();

        var longest = 0;
        var current = 0;
        DateOnly? previous = null;
        foreach (var date in dates)
        {
            current = previous is not null && date.DayNumber == previous.Value.DayNumber + 1
                ? current + 1
                : 1;
            longest = Math.Max(longest, current);
            previous = date;
        }

        return longest;
    }

    private static string GetHistoryIntensityLevel(int sessionCount, double? averageIntensityPercent)
    {
        if (sessionCount == 0)
            return "Rest";
        if (averageIntensityPercent >= 90)
            return "Peak";
        if (averageIntensityPercent >= 60)
            return "High";
        return "Moderate";
    }

    private static SessionHistoryItemDto ToHistoryItem(Session session)
    {
        var sets = session.Stations.SelectMany(station => station.Sets).ToList();
        return new SessionHistoryItemDto(
            session.Id,
            session.StartedAtUtc,
            session.EndedAtUtc,
            session.AthleteProtocolAssignment?.Protocol.Title,
            session.ProtocolDay?.Label,
            session.Stations.Count,
            sets.Count,
            session.TotalVolumeKg ?? sets.Sum(set => set.WeightKg * set.Reps),
            session.CaloriesEstimate,
            session.XpAwarded,
            session.IntensityPercent,
            session.Stations
                .OrderBy(station => station.SortOrder)
                .Select(station => new SessionHistoryStationDto(
                    station.MachineId,
                    station.Machine.Name,
                    station.Machine.MuscleGroup,
                    station.TargetSets,
                    station.RestSeconds,
                    station.SortOrder,
                    station.Sets
                        .OrderBy(set => set.SetNumber)
                        .Select(set => new SessionSetDto(
                            set.Id,
                            set.SetNumber,
                            set.WeightKg,
                            set.Reps,
                            set.LoggedAtUtc))
                        .ToList()))
                .ToList());
    }

    private static SessionDto ToSessionDto(Session session)
    {
        var liveVolume = session.Stations.SelectMany(st => st.Sets).Sum(s => s.WeightKg * s.Reps);
        var durationHours = ((session.EndedAtUtc ?? DateTime.UtcNow) - session.StartedAtUtc).TotalHours;
        var userWeightKg = session.User.CurrentWeightKg;
        int? liveCalories = userWeightKg is null
                            ? null
                            : (int)Math.Round(MetValueForResistanceTraining * (double)userWeightKg.Value * durationHours);

        return new SessionDto(
            session.Id, session.ProtocolDayId, session.ProtocolDay?.Label,
            session.StartedAtUtc, session.EndedAtUtc,
            liveVolume, liveCalories,
            session.Stations.OrderBy(st => st.SortOrder).Select(st => ToStationDto(st, st.Machine)).ToList());
    }

    private static SessionStationDto ToStationDto(SessionStation station, Machine machine)
        => new(station.Id, machine.Id, machine.Name, machine.MuscleGroup,
            station.TargetSets, station.RestSeconds, station.SortOrder,
            station.Sets.OrderBy(s => s.SetNumber)
                .Select(s => new SessionSetDto(s.Id, s.SetNumber, s.WeightKg, s.Reps, s.LoggedAtUtc))
                .ToList());
}
