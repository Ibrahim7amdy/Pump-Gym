namespace Pump.Application.Dtos.Protocols
{
    public record ActiveProtocolDto(
    int ProtocolId,
    int AssignmentId,
    string Title,
    string? CoachName,
    DateOnly AssignedOnUtc,
    DateOnly ExpiresOnUtc,
    decimal PaidPrice,
    bool IsOpenProtocol,
    int CurrentWeek,
    int TotalWeeks,
    double? ProgressPercent,
    int CompletedDaysCount,
    string Status,
    List<ProtocolDayProgressDto>? Days,
    ProtocolDayProgressDto? TodayProtocolDay,
    string? ExpectedGainLabel,
    string? ExpectedMuscleGainLabel,
    string? ExpectedStrengthGainLabel,
    int? ExpectedPoints,
    int? EstimatedCaloriesBurned,
    decimal PlannedVolumeKg,
    List<MuscleVolumeDto> PlannedVolumeByMuscleGroup,
    List<ProtocolWeekLoadDto> WeeklyLoadProgression);
}
