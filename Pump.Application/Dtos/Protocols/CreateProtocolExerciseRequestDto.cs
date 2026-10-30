namespace Pump.Application.Dtos.Protocols
{
    public record CreateProtocolExerciseRequestDto(
    int MachineId,
    int Sets,
    string RepsLabel,
    decimal TargetWeightKg,
    int RestSeconds,
    int SortOrder);
}
