namespace Pump.Application.Dtos.Users;

public record UpdateProfileRequestDto(
    string? FullName,
    string? Username,
    string? RankTitle,
    string? Bio,
    decimal? TargetWeightKg,
    decimal? CurrentWeightKg,
    decimal? HeightCm,
    int? CalorieGoal,
    decimal? WaterGoalLiters);