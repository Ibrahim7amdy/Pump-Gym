using Microsoft.EntityFrameworkCore;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Protocols;
using Pump.Application.Interfaces.Common;
using Pump.Application.Interfaces.Protocols;
using Pump.Domain.Entities;
using Pump.Domain.Enums;
using System.Text.RegularExpressions;

namespace Pump.Application.Services.Protocols;
public class ProtocolService : IProtocolService
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;

    public ProtocolService(IApplicationDbContext db, IFileStorageService fileStorage)
    {
        _db = db;
        _fileStorage = fileStorage;
    }

    public async Task<List<ProtocolListItemDto>> GetAllAsync(CancellationToken ct = default)
        => await _db.Protocols
            .Where(p => !p.IsArchivedVersion)
            .Select(p => new ProtocolListItemDto(
                p.Id, p.Title, p.Category, p.Difficulty.ToString(), p.Price, p.DurationDays,
                p.ExpectedGainLabel, p.ExpectedMuscleGainLabel, p.ExpectedStrengthGainLabel,
                p.ExpectedPoints, p.ImageUrl,
                p.CreatedByAdminUser != null ? p.CreatedByAdminUser.FullName : null))
            .ToListAsync(ct);

    public async Task<ProtocolDetailDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var protocol = await _db.Protocols
            .Where(p => !p.IsArchivedVersion)
            .Include(p => p.CreatedByAdminUser)
            .Include(p => p.Days).ThenInclude(d => d.Exercises).ThenInclude(e => e.Machine)
            .Include(p => p.WeeklyLoadPlan)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Protocol not found.");

        var assignmentProgressRows = await _db.AthleteProtocolAssignments
            .Where(a => a.Protocol.ProductFamilyId == protocol.ProductFamilyId)
            .Select(a => new
            {
                a.CompletedDaysCount,
                a.Protocol.DurationDays,
                WorkoutDayCount = a.Protocol.Days.Count(d => d.DayNumber.HasValue && d.Intensity != ProtocolDayIntensity.Rest)
            })
            .ToListAsync(ct);

        var enrolledCount = assignmentProgressRows.Count;
        var isOpenProtocol = !protocol.Days.Any(day => day.DayNumber.HasValue);
        var scheduledAssignmentProgressRows = assignmentProgressRows
            .Where(assignment => assignment.WorkoutDayCount > 0)
            .ToList();
        double? avgCompletion = isOpenProtocol
            ? null
            : scheduledAssignmentProgressRows.Count == 0
                ? 0
                : scheduledAssignmentProgressRows.Average(a =>
            {
                return Math.Min((double)a.CompletedDaysCount / a.WorkoutDayCount * 100, 100);
            });

        var totalSets = protocol.Days
            .SelectMany(d => d.Exercises)
            .Sum(e => e.Sets);

        var distinctMachines = protocol.Days
            .SelectMany(d => d.Exercises)
            .Select(e => e.MachineId)
            .Distinct()
            .Count();

        var trainingDays = protocol.Days.Count(d => d.Intensity != ProtocolDayIntensity.Rest);

        var sampleDay = protocol.Days
            .Where(d => d.Intensity != ProtocolDayIntensity.Rest)
            .OrderBy(d => d.DayNumber)
            .FirstOrDefault();

        var sampleExercises = sampleDay?.Exercises
            .OrderBy(e => e.SortOrder)
            .Select(e => new ProtocolExercisePreviewDto(e.Machine.Name, e.Sets, e.RepsLabel, e.TargetWeightKg))
            .ToList() ?? new List<ProtocolExercisePreviewDto>();
        var plannedVolumeByMuscleGroup = CalculatePlannedVolumeByMuscleGroup(protocol.Days);

        return new ProtocolDetailDto(
            protocol.Id, protocol.Title, protocol.Category, protocol.Difficulty.ToString(),
            protocol.Price, protocol.DurationDays, protocol.ExpectedGainLabel,
            protocol.ExpectedMuscleGainLabel, protocol.ExpectedStrengthGainLabel, protocol.ExpectedPoints,
            protocol.ImageUrl,
            protocol.CreatedByAdminUser?.FullName,
            enrolledCount, avgCompletion is null ? null : Math.Round(avgCompletion.Value, 0), totalSets, distinctMachines, trainingDays,
            sampleDay?.Label ?? "", sampleExercises,
            plannedVolumeByMuscleGroup.Sum(item => item.PlannedVolumeKg),
            plannedVolumeByMuscleGroup,
            protocol.WeeklyLoadPlan
                .Where(week => week.WeekNumber <= (int)Math.Ceiling(protocol.DurationDays / 7.0))
                .OrderBy(week => week.WeekNumber)
                .Select(week => new ProtocolWeekPlanDto(week.WeekNumber, week.PhaseLabel, week.LoadPercent))
                .ToList());
    }

    public async Task<List<ProtocolAchievementDto>> GetCompletedAsync(int userId, CancellationToken ct = default)
    {
        var assignments = await _db.AthleteProtocolAssignments
            .IgnoreQueryFilters()
            .Where(assignment => assignment.UserId == userId && assignment.Status == AssignmentStatus.Completed)
            .Include(assignment => assignment.Protocol)
            .OrderByDescending(assignment => assignment.AssignedOnUtc)
            .ThenByDescending(assignment => assignment.Id)
            .ToListAsync(ct);

        if (assignments.Count == 0)
            return [];

        var assignmentIds = assignments.Select(assignment => assignment.Id).ToList();
        var sessions = await _db.Sessions
            .IgnoreQueryFilters()
            .Where(session => session.AthleteProtocolAssignmentId != null
                && assignmentIds.Contains(session.AthleteProtocolAssignmentId.Value)
                && session.EndedAtUtc != null
                && session.Stations.Any(station => station.Sets.Any()))
            .Include(session => session.Stations).ThenInclude(station => station.Machine)
            .Include(session => session.Stations).ThenInclude(station => station.Sets)
            .ToListAsync(ct);

        return assignments.Select(assignment =>
        {
            var assignmentSessions = sessions
                .Where(session => session.AthleteProtocolAssignmentId == assignment.Id)
                .ToList();
            var sets = assignmentSessions
                .SelectMany(session => session.Stations)
                .SelectMany(station => station.Sets.Select(set => new { station.Machine, Set = set }))
                .ToList();
            var bestLift = sets.OrderByDescending(item => item.Set.WeightKg).FirstOrDefault();
            var intensities = assignmentSessions
                .Where(session => session.IntensityPercent.HasValue)
                .Select(session => session.IntensityPercent!.Value)
                .ToList();

            return new ProtocolAchievementDto(
                assignment.Id,
                assignment.ProtocolId,
                assignment.Protocol.Title,
                assignment.Protocol.ImageUrl,
                assignment.AssignedOnUtc,
                assignmentSessions.Max(session => session.EndedAtUtc),
                assignment.CompletedDaysCount,
                assignmentSessions.Select(session => session.Id).Distinct().Count(),
                assignmentSessions.Sum(session => session.TotalVolumeKg ?? 0),
                assignmentSessions.Sum(session => session.XpAwarded ?? 0),
                assignmentSessions.Sum(session => session.Stations.Count(station => station.Sets.Count > 0)),
                intensities.Count == 0 ? null : Math.Round(intensities.Average(), 0),
                bestLift?.Machine.Name,
                bestLift?.Machine.MuscleGroup,
                bestLift?.Set.WeightKg,
                GetLongestTrainingStreak(assignmentSessions.Select(session => session.EndedAtUtc!.Value)));
        }).ToList();
    }

    public async Task<ProtocolDetailDto> CreateAsync(int createdByAdminUserId, CreateProtocolRequestDto request, Stream? imageStream, string? imageContentType, CancellationToken ct = default)
    {
        var protocol = new Protocol
        {
            Title = request.Title,
            Category = request.Category,
            Difficulty = Enum.Parse<ProtocolDifficulty>(request.Difficulty),
            Price = request.Price,
            DurationDays = request.DurationDays,
            ExpectedGainLabel = request.ExpectedGainLabel,
            ExpectedMuscleGainLabel = request.ExpectedMuscleGainLabel,
            ExpectedStrengthGainLabel = request.ExpectedStrengthGainLabel,
            ExpectedPoints = request.ExpectedPoints,
            WeeklyLoadPlan = MapWeeklyLoadPlan(request.WeeklyLoadPlan),
            ImageUrl = imageStream is not null && imageContentType is not null
                ? await _fileStorage.SaveProtocolImageAsync(imageStream, imageContentType, ct)
                : request.ImageUrl,
            CreatedByAdminUserId = createdByAdminUserId,
            Days = request.Days.Select(d => new ProtocolDay
            {
                DayNumber = d.DayNumber,
                Label = d.Label,
                Intensity = Enum.Parse<ProtocolDayIntensity>(d.Intensity),
                Exercises = d.Exercises.Select(e => new ProtocolExercise
                {
                    MachineId = e.MachineId,
                    Sets = e.Sets,
                    RepsLabel = e.RepsLabel,
                    TargetWeightKg = e.TargetWeightKg,
                    RestSeconds = e.RestSeconds,
                    SortOrder = e.SortOrder
                }).ToList()
            }).ToList()
        };

        _db.Protocols.Add(protocol);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(protocol.Id, ct);
    }

    public async Task<ProtocolDetailDto> UpdateAsync(int id, UpdateProtocolRequestDto request, Stream? imageStream, string? imageContentType, CancellationToken ct = default)
    {
        var protocol = await _db.Protocols
            .Where(p => !p.IsArchivedVersion)
            .Include(p => p.Days).ThenInclude(d => d.Exercises)
            .Include(p => p.WeeklyLoadPlan)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Protocol not found.");

        var effectiveDurationDays = request.DurationDays ?? protocol.DurationDays;
        var effectiveDayNumbers = request.Days is null
            ? protocol.Days.Select(day => day.DayNumber)
            : request.Days.Select(day => day.DayNumber);
        if (effectiveDayNumbers.Any(dayNumber => dayNumber is int value && value > effectiveDurationDays))
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.Days)] = ["Scheduled day numbers must not exceed the protocol duration."]
            });
        }

        if (request.WeeklyLoadPlan?.Any(week => week.WeekNumber > (int)Math.Ceiling(effectiveDurationDays / 7.0)) == true)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.WeeklyLoadPlan)] = ["Weekly load plan weeks must be within the protocol duration."]
            });
        }

        var hasChanges = request.Title is not null
            || request.Category is not null
            || request.Difficulty is not null
            || request.Price is not null
            || request.DurationDays is not null
            || request.ExpectedGainLabel is not null
            || request.ExpectedMuscleGainLabel is not null
            || request.ExpectedStrengthGainLabel is not null
            || request.ExpectedPoints is not null
            || request.WeeklyLoadPlan is not null
            || request.ImageUrl is not null
            || request.Days is not null
            || imageStream is not null;
        if (!hasChanges)
            return await GetByIdAsync(id, ct);

        var previousDays = protocol.Days.ToList();
        var previousWeeklyLoadPlan = protocol.WeeklyLoadPlan.ToList();
        var archivedVersion = new Protocol
        {
            ProductFamilyId = protocol.ProductFamilyId,
            IsArchivedVersion = true,
            Title = protocol.Title,
            Price = protocol.Price,
            DurationDays = protocol.DurationDays,
            CreatedByAdminUserId = protocol.CreatedByAdminUserId,
            Category = protocol.Category,
            Difficulty = protocol.Difficulty,
            ExpectedGainLabel = protocol.ExpectedGainLabel,
            ExpectedMuscleGainLabel = protocol.ExpectedMuscleGainLabel,
            ExpectedStrengthGainLabel = protocol.ExpectedStrengthGainLabel,
            ExpectedPoints = protocol.ExpectedPoints,
            ImageUrl = protocol.ImageUrl,
            Days = previousDays,
            WeeklyLoadPlan = previousWeeklyLoadPlan
        };
        _db.Protocols.Add(archivedVersion);

        foreach (var day in previousDays)
            day.Protocol = archivedVersion;

        foreach (var week in previousWeeklyLoadPlan)
            week.Protocol = archivedVersion;

        var assignments = await _db.AthleteProtocolAssignments
            .Where(a => a.ProtocolId == id)
            .ToListAsync(ct);
        foreach (var assignment in assignments)
            assignment.Protocol = archivedVersion;

        protocol.Days = request.Days is not null
            ? MapDays(request.Days)
            : CloneDays(previousDays);
        protocol.WeeklyLoadPlan = request.WeeklyLoadPlan is not null
            ? MapWeeklyLoadPlan(request.WeeklyLoadPlan)
            : CloneWeeklyLoadPlan(previousWeeklyLoadPlan);

        if (request.Title is not null) protocol.Title = request.Title;
        if (request.Category is not null) protocol.Category = request.Category;
        if (request.Difficulty is not null) protocol.Difficulty = Enum.Parse<ProtocolDifficulty>(request.Difficulty);
        if (request.Price is not null) protocol.Price = request.Price.Value;
        if (request.DurationDays is not null) protocol.DurationDays = request.DurationDays.Value;
        if (request.ExpectedGainLabel is not null) protocol.ExpectedGainLabel = request.ExpectedGainLabel;
        if (request.ExpectedMuscleGainLabel is not null) protocol.ExpectedMuscleGainLabel = request.ExpectedMuscleGainLabel;
        if (request.ExpectedStrengthGainLabel is not null) protocol.ExpectedStrengthGainLabel = request.ExpectedStrengthGainLabel;
        if (request.ExpectedPoints is not null) protocol.ExpectedPoints = request.ExpectedPoints.Value;
        if (imageStream is not null && imageContentType is not null)
            protocol.ImageUrl = await _fileStorage.SaveProtocolImageAsync(imageStream, imageContentType, ct);
        else if (request.ImageUrl is not null)
            protocol.ImageUrl = request.ImageUrl;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var protocol = await _db.Protocols
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsArchivedVersion, ct)
            ?? throw new NotFoundException("Protocol not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var hasActiveAssignments = await _db.AthleteProtocolAssignments
            .AnyAsync(a => a.Protocol.ProductFamilyId == protocol.ProductFamilyId
                && a.Status == AssignmentStatus.Active
                && a.ExpiresOnUtc >= today, ct);
        if (hasActiveAssignments)
            throw new ConflictException("Cannot delete a protocol while an athlete has an active assignment.");

        protocol.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task AssignAsync(int protocolId, int assignedByUserId, AssignProtocolRequestDto request, CancellationToken ct = default)
    {
        var protocol = await _db.Protocols
            .FirstOrDefaultAsync(p => p.Id == protocolId && !p.IsArchivedVersion, ct)
            ?? throw new NotFoundException("Protocol not found.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && u.Role == UserRole.User, ct)
            ?? throw new NotFoundException("Athlete not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeAssignment = await _db.AthleteProtocolAssignments
            .FirstOrDefaultAsync(a => a.UserId == request.UserId && a.Status == AssignmentStatus.Active, ct);
        if (activeAssignment is not null)
        {
            var completedSessionCount = await _db.Sessions
                .Where(session => session.AthleteProtocolAssignmentId == activeAssignment.Id
                    && session.EndedAtUtc != null
                    && session.Stations.Any(station => station.Sets.Any()))
                .Select(session => session.Id)
                .Distinct()
                .CountAsync(ct);
            activeAssignment.CompletedDaysCount = completedSessionCount;

            var workoutDayCount = await _db.ProtocolDays
                .CountAsync(day => day.ProtocolId == activeAssignment.ProtocolId
                    && day.DayNumber.HasValue
                    && day.Intensity != ProtocolDayIntensity.Rest, ct);

            if (workoutDayCount > 0 && completedSessionCount >= workoutDayCount)
            {
                activeAssignment.Status = AssignmentStatus.Completed;
            }
            else if (activeAssignment.ExpiresOnUtc < today)
            {
                activeAssignment.Status = AssignmentStatus.Expired;
            }
            else if (activeAssignment.ProtocolId == protocolId)
            {
                throw new ConflictException("User is already assigned to this protocol.");
            }
            else
            {
                activeAssignment.Status = AssignmentStatus.Abandoned;
            }
        }

        _db.AthleteProtocolAssignments.Add(new AthleteProtocolAssignment
        {
            UserId = request.UserId,
            ProtocolId = protocolId,
            AssignedByUserId = assignedByUserId,
            AssignedOnUtc = today,
            ExpiresOnUtc = today.AddDays(protocol.DurationDays),
            PaidPrice = protocol.Price,
            Status = AssignmentStatus.Active
        });

        await _db.SaveChangesAsync(ct);
    }

    public async Task<ActiveProtocolDto> GetActiveAsync(int userId, CancellationToken ct = default)
    {
        var assignment = await _db.AthleteProtocolAssignments
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AssignedOnUtc)
            .ThenByDescending(a => a.Id)
            .Include(a => a.Protocol).ThenInclude(p => p.CreatedByAdminUser)
            .Include(a => a.Protocol).ThenInclude(p => p.Days).ThenInclude(d => d.Exercises).ThenInclude(e => e.Machine)
            .Include(a => a.Protocol).ThenInclude(p => p.WeeklyLoadPlan)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("No active protocol assigned.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var isOpenProtocol = !assignment.Protocol.Days.Any(day => day.DayNumber.HasValue);

        var totalWeeks = (int)Math.Ceiling(assignment.Protocol.DurationDays / 7.0);

        var daysSinceStart = DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - assignment.AssignedOnUtc.DayNumber + 1;
        var currentWeek = Math.Clamp((int)Math.Ceiling(daysSinceStart / 7.0), 1, totalWeeks);

        List<ProtocolDayProgressDto>? daysDto = null;
        ProtocolDayProgressDto? todayProtocolDay = null;
        double? progressPercent;
        var completedSessions = await _db.Sessions
            .Where(s => s.AthleteProtocolAssignmentId == assignment.Id
                && s.EndedAtUtc != null
                && s.Stations.Any(station => station.Sets.Any()))
            .Select(s => new { s.Id, s.StartedAtUtc, s.TotalVolumeKg, s.CaloriesEstimate, s.IntensityPercent })
            .ToListAsync(ct);
        var totalSessionsCompleted = completedSessions.Select(s => s.Id).Distinct().Count();

        if (assignment.CompletedDaysCount != totalSessionsCompleted)
        {
            assignment.CompletedDaysCount = totalSessionsCompleted;
        }

        if (!isOpenProtocol)
        {
            var completedDayIds = await _db.Sessions
                .Where(s => s.AthleteProtocolAssignmentId == assignment.Id
                    && s.ProtocolDayId != null
                    && s.EndedAtUtc != null
                    && s.Stations.Any(station => station.Sets.Any()))
                .Select(s => s.ProtocolDayId!.Value)
                .Distinct()
                .ToListAsync(ct);

            daysDto = assignment.Protocol.Days
                .OrderBy(d => d.DayNumber ?? int.MaxValue)
                .ThenBy(d => d.Id)
                .Select(d => new ProtocolDayProgressDto(
                    d.Id, d.DayNumber, d.Label, d.Intensity.ToString(),
                    IsCompleted: d.Intensity != ProtocolDayIntensity.Rest && completedDayIds.Contains(d.Id),
                    Exercises: d.Exercises.OrderBy(e => e.SortOrder)
                        .Select(e => new ProtocolDayExerciseDto(
                            e.Id, e.MachineId, e.Machine.Name, e.Machine.MuscleGroup,
                            e.Machine.ImageUrl, e.Machine.PinNumber, e.Sets, e.RepsLabel,
                            e.TargetWeightKg, e.RestSeconds, e.SortOrder))
                        .ToList()))
                .ToList();

            todayProtocolDay = daysDto.FirstOrDefault(day =>
                day.Intensity != ProtocolDayIntensity.Rest.ToString() && !day.IsCompleted);

        }

        var progressUnitCount = isOpenProtocol
            ? assignment.Protocol.DurationDays
            : assignment.Protocol.Days.Count(d => d.DayNumber.HasValue && d.Intensity != ProtocolDayIntensity.Rest);
        progressPercent = isOpenProtocol
            ? null
            : progressUnitCount == 0
                ? 0
            : Math.Round(Math.Min((double)assignment.CompletedDaysCount / progressUnitCount * 100, 100), 0);

        if (assignment.Status == AssignmentStatus.Active
            && !isOpenProtocol
            && progressPercent >= 100)
        {
            assignment.Status = AssignmentStatus.Completed;
        }

        if (assignment.Status == AssignmentStatus.Active && assignment.ExpiresOnUtc < today)
        {
            assignment.Status = AssignmentStatus.Expired;
        }

        if (assignment.Status is AssignmentStatus.Completed or AssignmentStatus.Expired or AssignmentStatus.Abandoned)
            todayProtocolDay = null;

        var plannedVolumeByMuscleGroup = isOpenProtocol
            ? []
            : CalculatePlannedVolumeByMuscleGroup(assignment.Protocol.Days);
        var plannedCycleVolume = plannedVolumeByMuscleGroup.Sum(item => item.PlannedVolumeKg);
        var completedSessionsByWeek = completedSessions
            .GroupBy(session => Math.Clamp(
                (DateOnly.FromDateTime(session.StartedAtUtc).DayNumber - assignment.AssignedOnUtc.DayNumber) / 7 + 1,
                1,
                totalWeeks))
            .ToDictionary(group => group.Key, group => group.ToList());
        var weeklyPlanByNumber = assignment.Protocol.WeeklyLoadPlan
            .Where(week => week.WeekNumber <= totalWeeks)
            .ToDictionary(week => week.WeekNumber);
        var weeklyLoadProgression = Enumerable.Range(1, totalWeeks)
            .Select(week =>
            {
                var sessions = completedSessionsByWeek.GetValueOrDefault(week) ?? [];
                var weeklyPlan = isOpenProtocol ? null : weeklyPlanByNumber.GetValueOrDefault(week);
                var intensities = sessions.Where(session => session.IntensityPercent.HasValue)
                    .Select(session => session.IntensityPercent!.Value)
                    .ToList();
                return new ProtocolWeekLoadDto(
                    week,
                    weeklyPlan?.PhaseLabel,
                    weeklyPlan?.LoadPercent,
                    weeklyPlan is null ? null : Math.Round(plannedCycleVolume * weeklyPlan.LoadPercent / 100, 2),
                    sessions.Count,
                    sessions.Sum(session => session.TotalVolumeKg ?? 0),
                    intensities.Count == 0 ? null : Math.Round(intensities.Average(), 0));
            })
            .ToList();

        if (_db.ChangeTracker.HasChanges())
            await _db.SaveChangesAsync(ct);

        return new ActiveProtocolDto(
            assignment.ProtocolId,
            assignment.Id,
            assignment.Protocol.Title,
            assignment.Protocol.CreatedByAdminUser?.FullName,
            assignment.AssignedOnUtc,
            assignment.ExpiresOnUtc,
            assignment.PaidPrice,
            isOpenProtocol,
            currentWeek, totalWeeks,
            progressPercent,
            totalSessionsCompleted,
            assignment.Status.ToString(),
            daysDto,
            todayProtocolDay,
            isOpenProtocol ? null : assignment.Protocol.ExpectedGainLabel,
            isOpenProtocol ? null : assignment.Protocol.ExpectedMuscleGainLabel,
            isOpenProtocol ? null : assignment.Protocol.ExpectedStrengthGainLabel,
            isOpenProtocol ? null : assignment.Protocol.ExpectedPoints,
            completedSessions.Any(session => session.CaloriesEstimate.HasValue)
                ? completedSessions.Sum(session => session.CaloriesEstimate ?? 0)
                : null,
            plannedCycleVolume,
            plannedVolumeByMuscleGroup,
            weeklyLoadProgression);
    }

    private static List<MuscleVolumeDto> CalculatePlannedVolumeByMuscleGroup(IEnumerable<ProtocolDay> days)
        => days
            .SelectMany(day => day.Exercises)
            .Select(exercise => new
            {
                exercise.Machine.MuscleGroup,
                Volume = GetAverageReps(exercise.RepsLabel) is int reps
                    ? exercise.TargetWeightKg * exercise.Sets * reps
                    : 0
            })
            .GroupBy(exercise => exercise.MuscleGroup)
            .Select(group => new MuscleVolumeDto(group.Key, group.Sum(exercise => exercise.Volume)))
            .OrderByDescending(item => item.PlannedVolumeKg)
            .ToList();

    private static int? GetAverageReps(string repsLabel)
    {
        var values = Regex.Matches(repsLabel, @"\d+")
            .Cast<Match>()
            .Select(match => int.TryParse(match.Value, out var value) ? value : (int?)null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();

        return values.Count == 0 ? null : (int)Math.Round(values.Average(), MidpointRounding.AwayFromZero);
    }

    private static int GetLongestTrainingStreak(IEnumerable<DateTime> completedAtUtc)
    {
        var trainingDates = completedAtUtc
            .Select(DateOnly.FromDateTime)
            .Distinct()
            .OrderBy(date => date)
            .ToList();

        var longestStreak = 0;
        var currentStreak = 0;
        DateOnly? previousDate = null;
        foreach (var date in trainingDates)
        {
            currentStreak = previousDate is not null && date.DayNumber == previousDate.Value.DayNumber + 1
                ? currentStreak + 1
                : 1;
            longestStreak = Math.Max(longestStreak, currentStreak);
            previousDate = date;
        }

        return longestStreak;
    }

    private static List<ProtocolWeekPlan> MapWeeklyLoadPlan(IEnumerable<CreateProtocolWeekPlanDto> plans)
        => plans.Select(week => new ProtocolWeekPlan
        {
            WeekNumber = week.WeekNumber,
            PhaseLabel = week.PhaseLabel,
            LoadPercent = week.LoadPercent
        }).ToList();

    private static List<ProtocolWeekPlan> CloneWeeklyLoadPlan(IEnumerable<ProtocolWeekPlan> plans)
        => plans.Select(week => new ProtocolWeekPlan
        {
            WeekNumber = week.WeekNumber,
            PhaseLabel = week.PhaseLabel,
            LoadPercent = week.LoadPercent
        }).ToList();

    private static List<ProtocolDay> MapDays(IEnumerable<CreateProtocolDayRequestDto> days)
        => days.Select(day => new ProtocolDay
        {
            DayNumber = day.DayNumber,
            Label = day.Label,
            Intensity = Enum.Parse<ProtocolDayIntensity>(day.Intensity),
            Exercises = day.Exercises.Select(exercise => new ProtocolExercise
            {
                MachineId = exercise.MachineId,
                Sets = exercise.Sets,
                RepsLabel = exercise.RepsLabel,
                TargetWeightKg = exercise.TargetWeightKg,
                RestSeconds = exercise.RestSeconds,
                SortOrder = exercise.SortOrder
            }).ToList()
        }).ToList();

    private static List<ProtocolDay> CloneDays(IEnumerable<ProtocolDay> days)
        => days.Select(day => new ProtocolDay
        {
            DayNumber = day.DayNumber,
            Label = day.Label,
            Intensity = day.Intensity,
            Exercises = day.Exercises.Select(exercise => new ProtocolExercise
            {
                MachineId = exercise.MachineId,
                Sets = exercise.Sets,
                RepsLabel = exercise.RepsLabel,
                TargetWeightKg = exercise.TargetWeightKg,
                RestSeconds = exercise.RestSeconds,
                SortOrder = exercise.SortOrder
            }).ToList()
        }).ToList();
}
