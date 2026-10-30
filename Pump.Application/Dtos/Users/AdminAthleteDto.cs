namespace Pump.Application.Dtos.Users;

public record AdminAthleteDto(
    int Id,
    string Username,
    string FullName,
    string? AvatarUrl,
    string? RankTitle,
    string? CoachName,
    DateTime CreatedAtUtc,
    bool IsVerified,
    AthleteProtocolSummaryDto? ActiveProtocol);
