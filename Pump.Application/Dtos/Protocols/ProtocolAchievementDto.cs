namespace Pump.Application.Dtos.Protocols;

public record ProtocolAchievementDto(
    int AssignmentId,
    int ProtocolId,
    string ProtocolTitle,
    string? ImageUrl,
    DateOnly AssignedOnUtc,
    DateTime? CompletedAtUtc,
    int CompletedDaysCount,
    int CompletedSessionsCount,
    decimal TotalVolumeKg,
    int TotalXp,
    int ExercisesCompleted,
    double? AverageIntensityPercent,
    string? BestLiftMachineName,
    string? BestLiftMuscleGroup,
    decimal? BestLiftWeightKg,
    int LongestTrainingStreakDays);
