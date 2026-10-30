namespace Pump.Application.Dtos.Protocols;

public record ProtocolDayExerciseDto(
    int ProtocolExerciseId,
    int MachineId,
    string MachineName,
    string MuscleGroup,
    string? ImageUrl,
    int PinNumber,
    int Sets,
    string RepsLabel,
    decimal TargetWeightKg,
    int RestSeconds,
    int SortOrder);
