
namespace Pump.Application.Dtos.Protocols
{
    public record UpdateProtocolRequestDto(
     string? Title,
     string? Category,
     string? Difficulty,
     decimal? Price,
     int? DurationDays,
     string? ExpectedGainLabel,
     string? ExpectedMuscleGainLabel,
     string? ExpectedStrengthGainLabel,
     int? ExpectedPoints,
     string? ImageUrl,
     List<CreateProtocolDayRequestDto>? Days,
     List<CreateProtocolWeekPlanDto>? WeeklyLoadPlan);
}
