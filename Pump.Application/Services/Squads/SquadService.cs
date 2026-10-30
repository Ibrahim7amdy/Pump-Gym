using Microsoft.EntityFrameworkCore;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Squads;
using Pump.Application.Interfaces.Common;
using Pump.Application.Interfaces.Squads;
using Pump.Domain.Entities;
using Pump.Domain.Enums;
using System.Security.Cryptography;

namespace Pump.Application.Services.Squads;

public sealed class SquadService : ISquadService
{
    private readonly IApplicationDbContext _db;

    public SquadService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<SquadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var squads = await _db.Squads.AsNoTracking()
            .Include(s => s.CreatedByUser)
            .Include(s => s.Members)
                .ThenInclude(member => member.User)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
        var userIds = squads.SelectMany(s => s.Members).Select(m => m.UserId).Distinct().ToArray();
        var monthStartUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var volumes = userIds.Length == 0
            ? new Dictionary<int, decimal>()
            : await _db.Sessions.AsNoTracking()
                .Where(session => userIds.Contains(session.UserId) && session.EndedAtUtc != null && session.StartedAtUtc >= monthStartUtc)
                .GroupBy(session => session.UserId)
                .Select(group => new { UserId = group.Key, VolumeKg = group.Sum(session => session.TotalVolumeKg ?? 0m) })
                .ToDictionaryAsync(x => x.UserId, x => x.VolumeKg / 1000m, ct);

        return squads.Select(squad =>
        {
            var members = squad.Members.Select(member => new SquadMemberDto(
                member.UserId,
                member.User.Username,
                member.User.FullName,
                member.User.AvatarUrl,
                member.User.RankTitle,
                volumes.GetValueOrDefault(member.UserId))).ToList();
            return new SquadDto(
                squad.Id,
                squad.Name,
                squad.InviteCode,
                squad.MonthlyGoalTonnage,
                members.Sum(member => member.MonthlyTonnage),
                members.Count,
                squad.CreatedByUserId,
                squad.CreatedByUser.FullName,
                squad.CreatedAtUtc,
                members);
        }).ToList();
    }

    public async Task<SquadDto> GetByIdAsync(int squadId, CancellationToken ct = default)
    {
        var squad = await _db.Squads.AsNoTracking()
            .Where(s => s.Id == squadId)
            .Select(s => new { s.Id, s.Name, s.InviteCode, s.MonthlyGoalTonnage, s.CreatedByUserId, CreatedByName = s.CreatedByUser.FullName, s.CreatedAtUtc })
            .FirstOrDefaultAsync(ct) ?? throw new NotFoundException("Squad not found.");

        var members = await _db.SquadMembers.AsNoTracking()
            .Where(m => m.SquadId == squadId)
            .Select(m => new { m.UserId, m.User.Username, m.User.FullName, m.User.AvatarUrl, m.User.RankTitle })
            .OrderBy(m => m.FullName)
            .ToListAsync(ct);

        var userIds = members.Select(m => m.UserId).ToArray();
        var monthStartUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var volumes = userIds.Length == 0
            ? new Dictionary<int, decimal>()
            : await _db.Sessions.AsNoTracking()
                .Where(session => userIds.Contains(session.UserId) && session.EndedAtUtc != null && session.StartedAtUtc >= monthStartUtc)
                .GroupBy(session => session.UserId)
                .Select(group => new { UserId = group.Key, VolumeKg = group.Sum(session => session.TotalVolumeKg ?? 0m) })
                .ToDictionaryAsync(x => x.UserId, x => x.VolumeKg / 1000m, ct);

        var memberDtos = members.Select(m => new SquadMemberDto(
            m.UserId, m.Username, m.FullName, m.AvatarUrl, m.RankTitle, volumes.GetValueOrDefault(m.UserId))).ToList();
        return new SquadDto(
            squad.Id, squad.Name, squad.InviteCode, squad.MonthlyGoalTonnage,
            memberDtos.Sum(m => m.MonthlyTonnage), memberDtos.Count,
            squad.CreatedByUserId, squad.CreatedByName, squad.CreatedAtUtc, memberDtos);
    }

    public async Task<IReadOnlyList<SquadDto>> GetForUserAsync(int userId, CancellationToken ct = default)
    {
        var squadIds = await _db.SquadMembers.AsNoTracking()
            .Where(member => member.UserId == userId)
            .OrderBy(member => member.JoinedAtUtc)
            .Select(member => member.SquadId)
            .ToListAsync(ct);
        var squads = new List<SquadDto>(squadIds.Count);
        foreach (var squadId in squadIds)
            squads.Add(await GetByIdAsync(squadId, ct));
        return squads;
    }

    public async Task<SquadDto> GetMemberViewAsync(int squadId, int userId, CancellationToken ct = default)
    {
        await EnsureMemberAsync(squadId, userId, ct);
        return await GetByIdAsync(squadId, ct);
    }

    public async Task<SquadDto> CreateAsync(int userId, CreateSquadRequestDto request, CancellationToken ct = default)
    {
        var athleteExists = await _db.Users.AnyAsync(user => user.Id == userId && user.Role == UserRole.User, ct);
        if (!athleteExists) throw new NotFoundException("Athlete not found.");

        var squad = new Squad
        {
            Name = request.Name.Trim(),
            MonthlyGoalTonnage = request.MonthlyGoalTonnage,
            InviteCode = await GenerateInviteCodeAsync(ct),
            CreatedByUserId = userId,
            Members = { new SquadMember { UserId = userId } }
        };
        _db.Squads.Add(squad);
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(squad.Id, ct);
    }

    public async Task<SquadDto> JoinAsync(int userId, string inviteCode, CancellationToken ct = default)
    {
        var normalizedCode = inviteCode.Trim().ToUpperInvariant();
        var squad = await _db.Squads.FirstOrDefaultAsync(s => s.InviteCode == normalizedCode, ct)
            ?? throw new NotFoundException("Squad invite code was not found.");
        await EnsureAthleteAsync(userId, ct);
        if (await _db.SquadMembers.AnyAsync(m => m.SquadId == squad.Id && m.UserId == userId, ct))
            throw new ConflictException("You are already a member of this squad.");

        _db.SquadMembers.Add(new SquadMember { SquadId = squad.Id, UserId = userId });
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(squad.Id, ct);
    }

    public async Task<SquadDto> UpdateNameAsync(int squadId, int userId, string name, CancellationToken ct = default)
    {
        var squad = await FindSquadAsync(squadId, ct);
        await EnsureMemberAsync(squadId, userId, ct);
        squad.Name = name.Trim();
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(squadId, ct);
    }

    public async Task DeleteAsync(int squadId, int userId, CancellationToken ct = default)
    {
        var squad = await FindSquadAsync(squadId, ct);
        if (squad.CreatedByUserId != userId)
            throw new ForbiddenException("Only the athlete who created this squad can delete it.");
        squad.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<SquadDto> AddMemberAsync(int squadId, int callerId, AddSquadMemberRequestDto request, CancellationToken ct = default)
    {
        _ = await FindSquadAsync(squadId, ct);
        await EnsureMemberAsync(squadId, callerId, ct);

        var username = request.Username?.Trim();
        if (!string.IsNullOrWhiteSpace(request.QrPayload))
        {
            const string userQrPrefix = "PUMP:USER:";
            var payload = request.QrPayload.Trim();
            if (!payload.StartsWith(userQrPrefix, StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(payload[userQrPrefix.Length..]))
                throw new ValidationAppException(new Dictionary<string, string[]>
                {
                    ["qrPayload"] = ["The QR code must contain a valid athlete code."]
                });
            username = payload[userQrPrefix.Length..].Trim();
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username && u.Role == UserRole.User, ct)
            ?? throw new NotFoundException("Athlete not found.");
        var alreadyMember = await _db.SquadMembers.AnyAsync(m => m.SquadId == squadId && m.UserId == user.Id, ct);
        if (alreadyMember) throw new ConflictException("Athlete is already a member of this squad.");

        _db.SquadMembers.Add(new SquadMember { SquadId = squadId, UserId = user.Id });
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(squadId, ct);
    }

    public async Task RemoveMemberAsync(int squadId, int callerId, int userId, CancellationToken ct = default)
    {
        var squad = await FindSquadAsync(squadId, ct);
        await EnsureMemberAsync(squadId, callerId, ct);
        var isOwner = squad.CreatedByUserId == callerId;
        if (!isOwner && userId != callerId)
            throw new ForbiddenException("Only the squad owner can remove another member. You can leave the squad yourself.");
        if (squad.CreatedByUserId == userId)
            throw new ConflictException("The squad creator cannot be removed. Delete the squad instead.");
        var member = await _db.SquadMembers.FirstOrDefaultAsync(m => m.SquadId == squadId && m.UserId == userId, ct)
            ?? throw new NotFoundException("Squad member not found.");
        _db.SquadMembers.Remove(member);
        await _db.SaveChangesAsync(ct);
    }

    private async Task EnsureMemberAsync(int squadId, int userId, CancellationToken ct)
    {
        if (!await _db.SquadMembers.AnyAsync(m => m.SquadId == squadId && m.UserId == userId, ct))
            throw new ForbiddenException("You must be a member of this squad to manage it.");
    }

    private async Task EnsureAthleteAsync(int userId, CancellationToken ct)
    {
        if (!await _db.Users.AnyAsync(user => user.Id == userId && user.Role == UserRole.User, ct))
            throw new NotFoundException("Athlete not found.");
    }

    private async Task<Squad> FindSquadAsync(int squadId, CancellationToken ct)
        => await _db.Squads.FirstOrDefaultAsync(s => s.Id == squadId, ct)
            ?? throw new NotFoundException("Squad not found.");

    private async Task<string> GenerateInviteCodeAsync(CancellationToken ct)
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var bytes = RandomNumberGenerator.GetBytes(6);
            var code = new string(bytes.Select(b => alphabet[b % alphabet.Length]).ToArray());
            if (!await _db.Squads.AnyAsync(s => s.InviteCode == code, ct)) return code;
        }
        throw new ConflictException("Could not generate a unique squad invite code. Try again.");
    }
}
