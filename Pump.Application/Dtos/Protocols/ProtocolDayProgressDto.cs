
namespace Pump.Application.Dtos.Protocols
{

    public record ProtocolDayProgressDto(
        int Id,
        int? DayNumber,
        string Label,
        string Intensity,
        bool IsCompleted,
        List<ProtocolDayExerciseDto> Exercises);
}
