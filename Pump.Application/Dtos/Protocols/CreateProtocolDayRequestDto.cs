
namespace Pump.Application.Dtos.Protocols
{
    public record CreateProtocolDayRequestDto(
    int? DayNumber,
    string Label,
    string Intensity,   // "Push" / "Pull" / "Legs" / "Upper" / "Rest"
    List<CreateProtocolExerciseRequestDto> Exercises);
}
