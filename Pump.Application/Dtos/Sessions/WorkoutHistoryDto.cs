namespace Pump.Application.Dtos.Sessions;

public record WorkoutHistoryDto(
    int TotalWorkouts,
    decimal TotalVolumeKg,
    double? AverageIntensityPercent,
    int LongestTrainingStreakDays,
    int RestDaysCount,
    ProtocolPersonalRecordHistoryDto PersonalRecords,
    List<SessionHistoryActivityDayDto> ActivityDays,
    List<SessionHistoryItemDto> Sessions,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public record SessionHistoryActivityDayDto(
    DateOnly DateUtc,
    int DayNumber,
    int CompletedSessionsCount,
    decimal TotalVolumeKg,
    double? AverageIntensityPercent,
    string IntensityLevel,
    List<SessionHistoryItemDto> Sessions);

public record SessionHistoryStationDto(
    int MachineId,
    string MachineName,
    string MuscleGroup,
    int TargetSets,
    int RestSeconds,
    int SortOrder,
    List<SessionSetDto> Sets);
