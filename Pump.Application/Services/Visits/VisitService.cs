using Microsoft.EntityFrameworkCore;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Visits;
using Pump.Application.Interfaces.Common;
using Pump.Application.Interfaces.Visits;
using Pump.Domain.Entities;
using Pump.Domain.Enums;

namespace Pump.Application.Services.Visits;

public class VisitService : IVisitService
{
    private const string GymQrPrefix = "PUMP:GYM:";
    private const string UserQrPrefix = "PUMP:USER:";
    private const int MaximumPageSize = 100;
    private readonly IApplicationDbContext _db;
    private readonly IGymQrSettings _gymQrSettings;
    private readonly TimeZoneInfo _gymTimeZone;
    private readonly IVisitEventPublisher _eventPublisher;

    public VisitService(IApplicationDbContext db, IGymQrSettings gymQrSettings, IVisitEventPublisher eventPublisher)
    {
        _db = db;
        _gymQrSettings = gymQrSettings;
        _gymTimeZone = gymQrSettings.GymTimeZone;
        _eventPublisher = eventPublisher;
    }

    public async Task<QrCodeDto> GetMyQrAsync(int userId, CancellationToken ct = default)
    {
        var username = await _db.Users
            .Where(user => user.Id == userId)
            .Select(user => user.Username)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("User not found.");

        return new QrCodeDto($"{UserQrPrefix}{username}");
    }

    public QrCodeDto GetGymQr()
    {
        return new QrCodeDto(_gymQrSettings.CreatePayload(), _gymQrSettings.GymName);
    }

    public async Task<AthleteQrPageDto> SearchAthleteQrsAsync(string? search, int page, int pageSize, CancellationToken ct = default)
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

        var query = _db.Users.AsNoTracking()
            .Where(user => user.Role == UserRole.User);

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
                IsCheckedIn = _db.Visits.Any(visit => visit.UserId == user.Id && visit.CheckOutAtUtc == null)
            })
            .ToListAsync(ct);

        return new AthleteQrPageDto(
            users.Select(user => new AthleteQrDto(
                user.Id,
                user.Username,
                user.FullName,
                user.AvatarUrl,
                $"{UserQrPrefix}{user.Username}",
                user.IsCheckedIn)).ToList(),
            page,
            pageSize,
            totalCount,
            totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<VisitDto> CheckInAsync(int callerId, bool isAdmin, CheckInRequestDto request, CancellationToken ct = default)
    {
        int userId;
        VisitSource source;
        int? staffUserId = null;

        if (_gymQrSettings.IsValidPayload(request.Payload))
        {
            userId = callerId;
            source = VisitSource.Self;
        }
        else if (request.Payload.StartsWith(UserQrPrefix, StringComparison.Ordinal))
        {
            if (!isAdmin)
                throw new ForbiddenException("Only an admin can check in another athlete's QR code.");

            var username = request.Payload[UserQrPrefix.Length..].Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(username))
                throw InvalidQrPayload();

            userId = await _db.Users
                .Where(user => user.Username == username)
                .Select(user => user.Id)
                .FirstOrDefaultAsync(ct);

            if (userId == 0)
                throw new NotFoundException("Athlete not found.");

            source = VisitSource.Admin;
            staffUserId = callerId;
        }
        else if (request.Payload.StartsWith(GymQrPrefix, StringComparison.Ordinal))
        {
            throw InvalidQrPayload();
        }
        else
        {
            throw InvalidQrPayload();
        }

        var alreadyInside = await _db.Visits
            .AnyAsync(visit => visit.UserId == userId && visit.CheckOutAtUtc == null, ct);
        if (alreadyInside)
            throw new ConflictException("This athlete already has an open gym visit.");

        var visit = new Visit
        {
            UserId = userId,
            CheckInAtUtc = DateTime.UtcNow,
            Source = source,
            CheckedInByStaffUserId = staffUserId
        };

        _db.Visits.Add(visit);
        await _db.SaveChangesAsync(ct);

        var result = await GetVisitDtoAsync(visit.Id, ct);
        await _eventPublisher.PublishCheckedInAsync(result, ct);
        return result;
    }

    public async Task<VisitDto> CreateWalkInAsync(CreateWalkInVisitRequestDto request, int adminUserId, CancellationToken ct = default)
    {
        var dropInFee = await _db.GymPricingSettings
            .Where(setting => setting.Id == 1)
            .Select(setting => (decimal?)setting.DropInFee)
            .SingleOrDefaultAsync(ct);
        if (dropInFee is null)
            throw new ConflictException("The admin must set the walk-in price before checking in a one-time visitor.");
        var visit = new Visit
        {
            GuestName = request.GuestName.Trim(),
            CheckInAtUtc = DateTime.UtcNow,
            Source = VisitSource.Walkin,
            DropInFee = dropInFee.Value,
            CheckedInByStaffUserId = adminUserId
        };

        _db.Visits.Add(visit);
        await _db.SaveChangesAsync(ct);

        var result = await GetVisitDtoAsync(visit.Id, ct);
        await _eventPublisher.PublishCheckedInAsync(result, ct);
        return result;
    }

    public async Task<VisitDto> CheckOutAsync(int visitId, int adminUserId, CancellationToken ct = default)
    {
        var checkedOutAtUtc = DateTime.UtcNow;
        var updatedCount = await _db.Visits
            .Where(item => item.Id == visitId && item.CheckOutAtUtc == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.CheckOutAtUtc, checkedOutAtUtc)
                .SetProperty(item => item.CheckedOutByStaffUserId, adminUserId)
                .SetProperty(item => item.UpdatedAtUtc, checkedOutAtUtc), ct);

        if (updatedCount == 0)
        {
            var visitExists = await _db.Visits.AnyAsync(item => item.Id == visitId, ct);
            if (!visitExists)
                throw new NotFoundException("Visit not found.");

            throw new ConflictException("This visit has already been checked out.");
        }

        var result = await GetVisitDtoAsync(visitId, ct);
        await _eventPublisher.PublishCheckedOutAsync(result, ct);
        return result;
    }

    public async Task<VisitDto> GetMyCurrentAsync(int userId, CancellationToken ct = default)
    {
        var visit = await _db.Visits
            .AsNoTracking()
            .Include(item => item.User)
            .FirstOrDefaultAsync(item => item.UserId == userId && item.CheckOutAtUtc == null, ct)
            ?? throw new NotFoundException("No open gym visit found.");

        return ToDto(visit);
    }

    public async Task<List<VisitDto>> GetCurrentAsync(CancellationToken ct = default)
    {
        var visits = await _db.Visits
            .AsNoTracking()
            .Include(item => item.User)
            .Where(item => item.CheckOutAtUtc == null)
            .OrderByDescending(item => item.CheckInAtUtc)
            .ToListAsync(ct);

        return visits.Select(ToDto).ToList();
    }

    public async Task<OccupancyDto> GetOccupancyAsync(CancellationToken ct = default)
    {
        var count = await _db.Visits.CountAsync(item => item.CheckOutAtUtc == null, ct);
        return new OccupancyDto(count);
    }

    public Task<VisitPageDto> GetMyHistoryAsync(int userId, DateOnly? date, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Visits
            .AsNoTracking()
            .Include(item => item.User)
            .Where(item => item.UserId == userId);

        if (date is DateOnly selectedDate)
        {
            var (start, end) = GetUtcDayRange(selectedDate);
            query = query.Where(item => item.CheckInAtUtc >= start && item.CheckInAtUtc < end);
        }
        else
        {
            var gymToday = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _gymTimeZone));
            var (from, _) = GetUtcDayRange(gymToday.AddDays(-29));
            query = query.Where(item => item.CheckInAtUtc >= from);
        }

        return CreatePageAsync(query, page, pageSize, ct);
    }

    public Task<VisitPageDto> GetHistoryAsync(DateOnly date, int page, int pageSize, int? userId = null, CancellationToken ct = default)
    {
        var (start, end) = GetUtcDayRange(date);
        var query = _db.Visits
            .AsNoTracking()
            .Include(item => item.User)
            .Where(item => item.CheckInAtUtc >= start && item.CheckInAtUtc < end);

        if (userId is not null)
            query = query.Where(item => item.UserId == userId.Value);

        return CreatePageAsync(query, page, pageSize, ct);
    }

    public async Task<List<DateOnly>> GetDatesWithActivityAsync(DateOnly month, int? userId, CancellationToken ct = default)
    {
        var (start, end) = GetUtcMonthRange(month);
        var query = _db.Visits
            .Where(visit => visit.CheckInAtUtc >= start && visit.CheckInAtUtc < end);

        if (userId is not null)
            query = query.Where(visit => visit.UserId == userId.Value);

        var checkInTimesUtc = await query
            .Select(visit => visit.CheckInAtUtc)
            .ToListAsync(ct);

        return checkInTimesUtc
            .Select(checkIn => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(checkIn, DateTimeKind.Utc), _gymTimeZone)))
            .Distinct()
            .OrderBy(date => date)
            .ToList();
    }

    private async Task<VisitPageDto> CreatePageAsync(IQueryable<Visit> query, int page, int pageSize, CancellationToken ct)
    {
        ValidatePagination(page, pageSize);

        var totalCount = await query.CountAsync(ct);
        var skip = (int)Math.Min((long)(page - 1) * pageSize, int.MaxValue);
        var items = await query
            .OrderByDescending(visit => visit.CheckInAtUtc)
            .ThenByDescending(visit => visit.Id)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        return new VisitPageDto(
            items.Select(ToDto).ToList(), page, pageSize, totalCount,
            totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    private async Task<VisitDto> GetVisitDtoAsync(int id, CancellationToken ct)
    {
        var visit = await _db.Visits
            .AsNoTracking()
            .Include(item => item.User)
            .FirstOrDefaultAsync(item => item.Id == id, ct)
            ?? throw new NotFoundException("Visit not found.");

        return ToDto(visit);
    }

    private static VisitDto ToDto(Visit visit)
        => new(
            visit.Id,
            visit.UserId,
            visit.User?.Username,
            visit.User?.FullName ?? visit.GuestName ?? "Walk-in guest",
            visit.CheckInAtUtc,
            visit.CheckOutAtUtc,
            visit.Source.ToString(),
            visit.UserId is null ? "One-Time Visitor" : "Registered Athlete",
            visit.UserId is not null,
            visit.CheckOutAtUtc is null,
            visit.CheckedInByStaffUserId,
            visit.CheckedOutByStaffUserId);

    private (DateTime Start, DateTime End) GetUtcDayRange(DateOnly date)
        => (ConvertGymDateToUtc(date), ConvertGymDateToUtc(date.AddDays(1)));

    private (DateTime Start, DateTime End) GetUtcMonthRange(DateOnly month)
    {
        var firstDay = new DateOnly(month.Year, month.Month, 1);
        return (ConvertGymDateToUtc(firstDay), ConvertGymDateToUtc(firstDay.AddMonths(1)));
    }

    private DateTime ConvertGymDateToUtc(DateOnly date)
    {
        var localMidnight = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(localMidnight, _gymTimeZone);
    }

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

    private static ValidationAppException InvalidQrPayload()
        => new(new Dictionary<string, string[]>
        {
            ["payload"] = ["The QR payload is invalid or not recognized."]
        });
}
