
namespace Pump.Application.Dtos.Machines
{
    public record MachineDto(
    int Id,
    string Name,
    string MuscleGroup,
    string Category,
    string ImageUrl,
    string? SafetyNotes,
    List<string> ExecutionSteps,
    List<MachineAlternativeDto> Alternatives,
    decimal? MyPersonalRecordKg,     // null لو الزائر مش مسجل دخول أو معندوش تاريخ على الماكينة دي
    decimal? MyLastWeightUsedKg);
}
