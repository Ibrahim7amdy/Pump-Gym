
namespace Pump.Application.Dtos.Protocols
{
    public record CreateProtocolRequestDto(
    string Title,
    string Category,
    string Difficulty,     // "Beginner" / "Intermediate" / "Advanced" / "AllLevels"
    decimal Price,
    int DurationDays,
    string? ExpectedGainLabel,
    string? ExpectedMuscleGainLabel,
    string? ExpectedStrengthGainLabel,
    int? ExpectedPoints,
    string? ImageUrl,
    List<CreateProtocolDayRequestDto> Days,
    List<CreateProtocolWeekPlanDto> WeeklyLoadPlan);
}
