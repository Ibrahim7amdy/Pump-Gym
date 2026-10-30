namespace Pump.Application.Dtos.Protocols
{
    public record ProtocolExercisePreviewDto(
    string MachineName,
    int Sets,
    string RepsLabel,
    decimal TargetWeightKg);
}
