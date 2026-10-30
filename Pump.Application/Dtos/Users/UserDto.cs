namespace Pump.Application.Dtos.Users;

public record UserDto(
    int Id,
    string Username,
    string Email,
    string FullName,
    string Role,
    string? RankTitle,
    string? CoachName,
    string? Bio,
    string? AvatarUrl,
    decimal? TargetWeightKg,
    decimal? CurrentWeightKg,
    decimal? HeightCm,
    int? CalorieGoal,
    decimal? WaterGoalLiters,
    int Xp,
    string PreferredLanguage,
    string PreferredThemeId,
    bool IsVerified,
    DateTime CreatedAtUtc);
