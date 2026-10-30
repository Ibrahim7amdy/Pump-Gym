namespace Pump.Application.Dtos.Protocols;

public record ProtocolWeekLoadDto(
    int WeekNumber,
    string? PhaseLabel,
    decimal? PlannedLoadPercent,
    decimal? PlannedVolumeKg,
    int CompletedSessionsCount,
    decimal ActualVolumeKg,
    double? AverageIntensityPercent);
