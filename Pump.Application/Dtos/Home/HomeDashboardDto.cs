using Pump.Application.Dtos.Protocols;

namespace Pump.Application.Dtos.Home;

public sealed record HomeDashboardDto(
    int UserId,
    string DisplayName,
    string Username,
    string? AvatarUrl,
    string PreferredLanguage,
    string PreferredThemeId,
    int Xp,
    string Tier,
    int Level,
    double LevelProgressPercent,
    bool IsCheckedIn,
    int AthletesInGymCount,
    int ActiveMachinesCount,
    int UnreadNotificationsCount,
    HomeActiveProtocolDto? ActiveProtocol,
    HomeCurrentSessionDto? CurrentSession);

public sealed record HomeActiveProtocolDto(
    int ProtocolId,
    int AssignmentId,
    string Title,
    string? ImageUrl,
    string? CoachName,
    string Status,
    bool IsOpenProtocol,
    int CurrentWeek,
    int TotalWeeks,
    double? ProgressPercent,
    int CompletedDaysCount,
    ProtocolDayProgressDto? TodayProtocolDay,
    string? ExpectedGainLabel,
    string? ExpectedMuscleGainLabel,
    string? ExpectedStrengthGainLabel,
    int? ExpectedPoints,
    int? EstimatedCaloriesBurned,
    decimal PlannedVolumeKg,
    IReadOnlyList<MuscleVolumeDto> PlannedVolumeByMuscleGroup,
    IReadOnlyList<ProtocolWeekLoadDto> WeeklyLoadProgression);

public sealed record HomeCurrentSessionDto(
    int Id,
    DateTime StartedAtUtc,
    int? ProtocolDayId,
    string? ProtocolDayLabel,
    int CompletedSetsCount,
    IReadOnlyList<HomeCurrentSessionStationDto> Stations);

public sealed record HomeCurrentSessionStationDto(
    int Id,
    int MachineId,
    string MachineName,
    int TargetSets,
    int CompletedSetsCount);
