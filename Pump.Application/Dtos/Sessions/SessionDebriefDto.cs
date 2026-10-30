namespace Pump.Application.Dtos.Sessions
{
    public record SessionDebriefDto(
     int SessionId,
     int XpAwarded,
     int Level,
     double LevelProgressPercent,
     double IntensityPercent,
     string Duration,
     decimal TotalVolumeKg,
     int? CaloriesEstimate);
}
