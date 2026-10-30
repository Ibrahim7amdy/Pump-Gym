using Microsoft.EntityFrameworkCore;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Users;
using Pump.Application.Interfaces.Common;
using Pump.Application.Interfaces.Users;
using Pump.Domain.Entities;
using Pump.Domain.Enums;

namespace Pump.Application.Services.Users;

public class AdminAthleteService : IAdminAthleteService
{
    private const int MaximumPageSize = 100;
    private readonly IApplicationDbContext _db;

    public AdminAthleteService(IApplicationDbContext db) => _db = db;

    public async Task<AdminAthletePageDto> GetAthletesAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        ValidatePagination(page, pageSize);
        var normalizedSearch = search?.Trim();
        if (normalizedSearch?.Length > 100)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["search"] = ["Search must not exceed 100 characters."]
            });
        }

        var query = _db.Users.AsNoTracking().Where(user => user.Role == UserRole.User);
        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(user => user.Username.Contains(normalizedSearch)
                || user.FullName.Contains(normalizedSearch));
        }

        var totalCount = await query.CountAsync(ct);
        var skip = (int)Math.Min((long)(page - 1) * pageSize, int.MaxValue);
        var users = await query
            .OrderBy(user => user.FullName)
            .ThenBy(user => user.Id)
            .Skip(skip)
            .Take(pageSize)
            .Select(user => new
            {
                user.Id,
                user.Username,
                user.FullName,
                user.AvatarUrl,
                user.RankTitle,
                user.CoachName,
                user.CreatedAtUtc,
                user.IsVerified
            })
            .ToListAsync(ct);

        var userIds = users.Select(user => user.Id).ToList();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var assignments = await _db.AthleteProtocolAssignments
            .AsNoTracking()
            .Include(assignment => assignment.Protocol)
            .Where(assignment => userIds.Contains(assignment.UserId)
                && assignment.Status == AssignmentStatus.Active
                && assignment.ExpiresOnUtc >= today)
            .ToListAsync(ct);
        var activeByUserId = assignments
            .GroupBy(assignment => assignment.UserId)
            .ToDictionary(group => group.Key, group => group
                .OrderByDescending(assignment => assignment.AssignedOnUtc)
                .Select(assignment => new AthleteProtocolSummaryDto(
                    assignment.Id,
                    assignment.ProtocolId,
                    assignment.Protocol.Title,
                    assignment.AssignedOnUtc,
                    assignment.ExpiresOnUtc,
                    assignment.PaidPrice))
                .First());

        var items = users.Select(user => new AdminAthleteDto(
            user.Id,
            user.Username,
            user.FullName,
            user.AvatarUrl,
            user.RankTitle,
            user.CoachName,
            user.CreatedAtUtc,
            user.IsVerified,
            activeByUserId.GetValueOrDefault(user.Id))).ToList();

        return new AdminAthletePageDto(
            items,
            page,
            pageSize,
            totalCount,
            totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<AdminAthleteDto> SetVerificationAsync(int athleteId, bool isVerified, CancellationToken ct = default)
    {
        var athlete = await FindAthleteAsync(athleteId, ct);
        athlete.IsVerified = isVerified;
        await _db.SaveChangesAsync(ct);
        return await GetAthleteAsync(athleteId, ct);
    }

    public async Task<AdminAthleteDto> UpdateCoachAsync(int athleteId, string? coachName, CancellationToken ct = default)
    {
        var athlete = await FindAthleteAsync(athleteId, ct);
        athlete.CoachName = string.IsNullOrWhiteSpace(coachName) ? null : coachName.Trim();
        await _db.SaveChangesAsync(ct);
        return await GetAthleteAsync(athleteId, ct);
    }

    private async Task<AdminAthleteDto> GetAthleteAsync(int athleteId, CancellationToken ct)
    {
        var athlete = await _db.Users.AsNoTracking()
            .Where(user => user.Id == athleteId && user.Role == UserRole.User)
            .Select(user => new
            {
                user.Id,
                user.Username,
                user.FullName,
                user.AvatarUrl,
                user.RankTitle,
                user.CoachName,
                user.CreatedAtUtc,
                user.IsVerified
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("Athlete not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeProtocol = await _db.AthleteProtocolAssignments.AsNoTracking()
            .Where(assignment => assignment.UserId == athleteId
                && assignment.Status == AssignmentStatus.Active
                && assignment.ExpiresOnUtc >= today)
            .OrderByDescending(assignment => assignment.AssignedOnUtc)
            .Select(assignment => new AthleteProtocolSummaryDto(
                assignment.Id,
                assignment.ProtocolId,
                assignment.Protocol.Title,
                assignment.AssignedOnUtc,
                assignment.ExpiresOnUtc,
                assignment.PaidPrice))
            .FirstOrDefaultAsync(ct);

        return new AdminAthleteDto(
            athlete.Id,
            athlete.Username,
            athlete.FullName,
            athlete.AvatarUrl,
            athlete.RankTitle,
            athlete.CoachName,
            athlete.CreatedAtUtc,
            athlete.IsVerified,
            activeProtocol);
    }

    private async Task<User> FindAthleteAsync(int athleteId, CancellationToken ct)
        => await _db.Users.FirstOrDefaultAsync(user => user.Id == athleteId && user.Role == UserRole.User, ct)
            ?? throw new NotFoundException("Athlete not found.");

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1 || pageSize is < 1 or > MaximumPageSize)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["pagination"] = [$"Page must be at least 1 and pageSize must be between 1 and {MaximumPageSize}."]
            });
        }
    }
}
