namespace Pump.Application.Dtos.Sessions
{
    public record SessionDto(
    int Id,
    int? ProtocolDayId,
    string? ProtocolDayLabel,       // "Push Day — 29" مثلًا
    DateTime StartedAtUtc,
    DateTime? EndedAtUtc,
    decimal TotalVolumeKg,           // بيتحدث كل ما يتسجل Set جديد
    int? CaloriesEstimate,            // تقدير حي، بيتحدث بردو
    List<SessionStationDto> Stations);
}
