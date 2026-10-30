namespace Pump.Application.Dtos.Sessions;

public record SessionHistoryItemDto(
    int Id,
    DateTime StartedAtUtc,
    DateTime? EndedAtUtc,
    string? ProtocolTitle,
    string? ProtocolDayLabel,
    int StationCount,
    int SetCount,
    decimal TotalVolumeKg,
    int? CaloriesEstimate,
    int? XpAwarded,
    double? IntensityPercent,
    List<SessionHistoryStationDto> Stations);
