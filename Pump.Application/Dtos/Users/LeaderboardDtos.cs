namespace Pump.Application.Dtos.Users;

public record LeaderboardItemDto(
    int Rank,
    int UserId,
    string DisplayName,
    string Username,
    string? AvatarUrl,
    string Tier,
    int Points,
    bool IsVerified);
