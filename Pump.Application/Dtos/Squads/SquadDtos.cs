namespace Pump.Application.Dtos.Squads;

public sealed record SquadMemberDto(
    int UserId,
    string Username,
    string FullName,
    string? AvatarUrl,
    string? RankTitle,
    decimal MonthlyTonnage);

public sealed record SquadDto(
    int Id,
    string Name,
    string InviteCode,
    decimal MonthlyGoalTonnage,
    decimal MonthlyTonnage,
    int MemberCount,
    int CreatedByUserId,
    string CreatedByName,
    DateTime CreatedAtUtc,
    IReadOnlyList<SquadMemberDto> Members);

public sealed record CreateSquadRequestDto(string Name, decimal MonthlyGoalTonnage);
public sealed record UpdateSquadRequestDto(string Name);
public sealed record JoinSquadRequestDto(string InviteCode);
public sealed record AddSquadMemberRequestDto(string? Username, string? QrPayload);
