
namespace Pump.Application.Dtos.Protocols
{
    public record ProtocolDetailDto(
    int Id,
    string Title,
    string Category,
    string Difficulty,
    decimal Price,
    int DurationDays,
    string? ExpectedGainLabel,
    string? ExpectedMuscleGainLabel,
    string? ExpectedStrengthGainLabel,
    int? ExpectedPoints,
    string? ImageUrl,
    string? CoachName,
    int EnrolledCount,
    double? AverageCompletionPercent,
    int TotalSessions,
    int DistinctMachinesCount,
    int TotalTrainingDays,
    string SampleDayLabel,
    List<ProtocolExercisePreviewDto> SampleDayExercises,
    decimal PlannedVolumeKg,
    List<MuscleVolumeDto> PlannedVolumeByMuscleGroup,
    List<ProtocolWeekPlanDto> WeeklyLoadPlan);
}
