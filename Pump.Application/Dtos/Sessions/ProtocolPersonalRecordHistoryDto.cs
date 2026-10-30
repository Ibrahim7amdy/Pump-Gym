namespace Pump.Application.Dtos.Sessions;

public record ProtocolPersonalRecordHistoryDto(
    int? AssignmentId,
    int? ProtocolId,
    string? ProtocolTitle,
    int TotalPersonalRecords,
    List<PersonalRecordAchievementDto> Records);

public record PersonalRecordAchievementDto(
    int SessionId,
    int MachineId,
    string MachineName,
    string MuscleGroup,
    string? ProtocolDayLabel,
    decimal? PreviousRecordKg,
    decimal RecordWeightKg,
    int Reps,
    DateTime AchievedAtUtc);
