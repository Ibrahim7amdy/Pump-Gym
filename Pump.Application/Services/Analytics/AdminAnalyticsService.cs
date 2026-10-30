using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Pump.Application.Dtos.Analytics;
using Pump.Application.Interfaces.Analytics;
using Pump.Application.Interfaces.Common;
using Pump.Application.Interfaces.Visits;
using Pump.Domain.Enums;

namespace Pump.Application.Services.Analytics;

public sealed class AdminAnalyticsService : IAdminAnalyticsService
{
    private const int GrowthMonths = 6;
    private const int MachineUsageDays = 30;
    private const int GymOpenHoursPerDay = 15;

    private readonly IApplicationDbContext _db;
    private readonly TimeZoneInfo _gymTimeZone;

    public AdminAnalyticsService(IApplicationDbContext db, IGymQrSettings gymQrSettings)
    {
        _db = db;
        _gymTimeZone = gymQrSettings.GymTimeZone;
    }

    public async Task<AdminAnalyticsDashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var nowUtc = DateTime.UtcNow;
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, _gymTimeZone);
        var today = DateOnly.FromDateTime(localNow);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var nextMonthStart = monthStart.AddMonths(1);
        var monthStartUtc = ToUtc(monthStart);
        var nextMonthStartUtc = ToUtc(nextMonthStart);

        var grossRevenue = await _db.AthleteProtocolAssignments
            .AsNoTracking()
            .SumAsync(assignment => (decimal?)assignment.PaidPrice, ct) ?? 0m;

        var grossDropInRevenue = await _db.Visits
            .AsNoTracking()
            .Where(visit => visit.Source == VisitSource.Walkin)
            .SumAsync(visit => (decimal?)visit.DropInFee, ct) ?? 0m;

        var monthAssignments = await _db.AthleteProtocolAssignments
            .AsNoTracking()
            .Where(assignment => assignment.AssignedOnUtc >= monthStart
                && assignment.AssignedOnUtc < nextMonthStart)
            .Select(assignment => new { assignment.ProtocolId, assignment.PaidPrice })
            .ToListAsync(ct);

        var thisMonthVisitorCheckIns = await _db.Visits
            .AsNoTracking()
            .CountAsync(visit => visit.Source == VisitSource.Walkin
                && visit.CheckInAtUtc >= monthStartUtc
                && visit.CheckInAtUtc < nextMonthStartUtc, ct);
        var dropInRevenueThisMonth = await _db.Visits
            .AsNoTracking()
            .Where(visit => visit.Source == VisitSource.Walkin
                && visit.CheckInAtUtc >= monthStartUtc
                && visit.CheckInAtUtc < nextMonthStartUtc)
            .SumAsync(visit => (decimal?)visit.DropInFee, ct) ?? 0m;
        var currentDropInFee = await _db.GymPricingSettings
            .AsNoTracking()
            .Where(setting => setting.Id == 1)
            .Select(setting => (decimal?)setting.DropInFee)
            .SingleOrDefaultAsync(ct);

        var totalGrossRevenue = grossRevenue + grossDropInRevenue;
        var distribution = new[]
            {
                new { Category = "Drop-ins", Amount = grossDropInRevenue },
                new { Category = "Plans", Amount = grossRevenue },
                new { Category = "Other", Amount = 0m }
            }
            .Select(item => new RevenueDistributionDto(
                item.Category,
                item.Amount,
                totalGrossRevenue == 0 ? 0 : Math.Round((double)(item.Amount / totalGrossRevenue * 100m), 1)))
            .ToList();

        var growthStart = monthStart.AddMonths(-(GrowthMonths - 1));
        var growthStartUtc = ToUtc(growthStart);
        var growthEndUtc = ToUtc(nextMonthStart);
        var athleteCreatedDates = await _db.Users
            .AsNoTracking()
            .Where(user => user.Role == UserRole.User
                && user.CreatedAtUtc >= growthStartUtc
                && user.CreatedAtUtc < growthEndUtc)
            .Select(user => user.CreatedAtUtc)
            .ToListAsync(ct);
        var growthByMonth = athleteCreatedDates
            .Select(createdAt => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(createdAt, _gymTimeZone)))
            .GroupBy(date => new DateOnly(date.Year, date.Month, 1))
            .ToDictionary(group => group.Key, group => group.Count());
        var athleteGrowth = Enumerable.Range(0, GrowthMonths)
            .Select(offset => growthStart.AddMonths(offset))
            .Select(month => new AthleteGrowthMonthDto(
                month.ToString("yyyy-MM", CultureInfo.InvariantCulture),
                growthByMonth.GetValueOrDefault(month)))
            .ToList();

        var verificationGroups = await _db.Users
            .AsNoTracking()
            .Where(user => user.Role == UserRole.User)
            .GroupBy(user => user.IsVerified)
            .Select(group => new { IsVerified = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.IsVerified, item => item.Count, ct);

        var squadMembers = await _db.SquadMembers
            .AsNoTracking()
            .Select(member => new { member.SquadId, member.Squad.Name, member.UserId })
            .ToListAsync(ct);
        var squadUserIds = squadMembers.Select(member => member.UserId).Distinct().ToArray();
        var monthlyVolumes = squadUserIds.Length == 0
            ? new Dictionary<int, decimal>()
            : await _db.Sessions
                .AsNoTracking()
                .Where(session => squadUserIds.Contains(session.UserId)
                    && session.EndedAtUtc != null
                    && session.StartedAtUtc >= monthStartUtc
                    && session.StartedAtUtc < nextMonthStartUtc)
                .GroupBy(session => session.UserId)
                .Select(group => new { UserId = group.Key, VolumeKg = group.Sum(session => session.TotalVolumeKg ?? 0m) })
                .ToDictionaryAsync(item => item.UserId, item => item.VolumeKg, ct);
        var squadEngagement = squadMembers
            .GroupBy(member => new { member.SquadId, member.Name })
            .Select(group => new SquadEngagementDto(
                group.Key.SquadId,
                group.Key.Name,
                Math.Round(group.Sum(member => monthlyVolumes.GetValueOrDefault(member.UserId)) / 1000m, 1)))
            .OrderByDescending(squad => squad.MonthlyTonnage)
            .ToList();

        var weekStart = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var weekEnd = weekStart.AddDays(7);
        var weekCheckIns = await _db.Visits
            .AsNoTracking()
            .Where(visit => visit.CheckInAtUtc >= ToUtc(weekStart)
                && visit.CheckInAtUtc < ToUtc(weekEnd))
            .Select(visit => visit.CheckInAtUtc)
            .ToListAsync(ct);
        var checkInsByDay = weekCheckIns
            .Select(checkIn => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(checkIn, _gymTimeZone)))
            .GroupBy(date => date)
            .ToDictionary(group => group.Key, group => group.Count());
        var weeklyPeakTraffic = Enumerable.Range(0, 7)
            .Select(offset => weekStart.AddDays(offset))
            .Select(date => new DailyTrafficDto(
                date.ToString("ddd", CultureInfo.InvariantCulture),
                checkInsByDay.GetValueOrDefault(date)))
            .ToList();

        var todayVisits = await _db.Visits
            .AsNoTracking()
            .Where(visit => visit.CheckInAtUtc >= ToUtc(today)
                && visit.CheckInAtUtc < ToUtc(today.AddDays(1)))
            .Select(visit => visit.CheckInAtUtc)
            .ToListAsync(ct);
        var floorTrafficFrom = today.AddDays(-30);
        var floorTrafficStartUtc = ToUtc(floorTrafficFrom);
        var floorTrafficEndUtc = ToUtc(today);
        var occupancyVisits = await _db.Visits
            .AsNoTracking()
            .Where(visit => visit.CheckInAtUtc < floorTrafficEndUtc
                && (visit.CheckOutAtUtc == null || visit.CheckOutAtUtc > floorTrafficStartUtc))
            .Select(visit => new { visit.CheckInAtUtc, visit.CheckOutAtUtc })
            .ToListAsync(ct);
        var floorTraffic = Enumerable.Range(6, 17)
            .Select(hour =>
            {
                var personMinutes = 0d;
                for (var offset = 0; offset < 30; offset++)
                {
                    var day = floorTrafficFrom.AddDays(offset);
                    var bucketStart = ToUtc(day).AddHours(hour);
                    var bucketEnd = bucketStart.AddHours(1);
                    personMinutes += occupancyVisits.Sum(visit =>
                    {
                        var start = visit.CheckInAtUtc > bucketStart ? visit.CheckInAtUtc : bucketStart;
                        var actualEnd = visit.CheckOutAtUtc ?? nowUtc;
                        var end = actualEnd < bucketEnd ? actualEnd : bucketEnd;
                        return end > start ? (end - start).TotalMinutes : 0d;
                    });
                }

                return new HourlyTrafficDto($"{hour:00}:00", Math.Round((decimal)(personMinutes / (30 * 60d)), 2));
            })
            .ToList();

        var currentOccupancy = await _db.Visits
            .AsNoTracking()
            .CountAsync(visit => visit.CheckOutAtUtc == null, ct);

        var machineUsageFrom = today.AddDays(-(MachineUsageDays - 1));
        var machineUsageStartUtc = ToUtc(machineUsageFrom);
        var machineUsageEndUtc = ToUtc(today.AddDays(1));
        var machineSetRows = await _db.SessionSets
            .AsNoTracking()
            .Where(set => set.SessionStation.Session.EndedAtUtc != null
                && set.SessionStation.Session.StartedAtUtc >= machineUsageStartUtc
                && set.SessionStation.Session.StartedAtUtc < machineUsageEndUtc)
            .Select(set => new
            {
                SessionId = set.SessionStation.SessionId,
                set.SessionStation.Session.StartedAtUtc,
                set.SessionStation.Session.EndedAtUtc,
                set.SessionStation.MachineId,
                MachineName = set.SessionStation.Machine.Name,
                set.LoggedAtUtc
            })
            .ToListAsync(ct);
        var machineMetrics = new Dictionary<int, MachineUsageAccumulator>();
        foreach (var sessionGroup in machineSetRows.GroupBy(row => row.SessionId))
        {
            var machineGroups = sessionGroup.GroupBy(row => row.MachineId).ToList();
            var session = sessionGroup.First();
            if (machineGroups.Count == 0 || session.EndedAtUtc is null)
                continue;

            var durationHours = Math.Max(0, (session.EndedAtUtc.Value - session.StartedAtUtc).TotalHours);
            foreach (var machineGroup in machineGroups)
            {
                var machineName = machineGroup.First().MachineName;
                if (!machineMetrics.TryGetValue(machineGroup.Key, out var accumulator))
                {
                    accumulator = new MachineUsageAccumulator(machineGroup.Key, machineName);
                    machineMetrics.Add(machineGroup.Key, accumulator);
                }

                accumulator.SessionIds.Add(sessionGroup.Key);
                var machineSetTimes = machineGroup.Select(row => row.LoggedAtUtc).Order().ToList();
                if (machineSetTimes.Count >= 2)
                {
                    var activeSpanHours = Math.Max(0, (machineSetTimes[^1] - machineSetTimes[0]).TotalHours);
                    accumulator.EstimatedHours += Math.Min(activeSpanHours, durationHours);
                }
                foreach (var row in machineGroup)
                {
                    var localSetTime = TimeZoneInfo.ConvertTimeFromUtc(row.LoggedAtUtc, _gymTimeZone);
                    accumulator.SetHours[localSetTime.Hour] = accumulator.SetHours.GetValueOrDefault(localSetTime.Hour) + 1;
                }
            }
        }

        var machineUsage = machineMetrics.Values
            .OrderByDescending(item => item.SessionIds.Count)
            .ThenBy(item => item.Name)
            .Take(10)
            .Select(item =>
            {
                var peakHour = item.SetHours.Count == 0
                    ? (int?)null
                    : item.SetHours.OrderByDescending(pair => pair.Value).ThenBy(pair => pair.Key).First().Key;
                var elapsedDays = today.DayNumber - machineUsageFrom.DayNumber + 1;
                var availableHours = elapsedDays * GymOpenHoursPerDay;
                var utilization = availableHours == 0
                    ? 0
                    : Math.Min(100, item.EstimatedHours / availableHours * 100);
                return new MachineUsageDto(
                    item.Id,
                    item.Name,
                    item.SessionIds.Count,
                    Math.Round((decimal)item.EstimatedHours, 1),
                    Math.Round(utilization, 1),
                    peakHour is null ? null : $"{peakHour:00}:00");
            })
            .ToList();

        var protocolSales = await _db.AthleteProtocolAssignments
            .AsNoTracking()
            .GroupBy(assignment => assignment.ProtocolId)
            .Select(group => new
            {
                ProtocolId = group.Key,
                Assignments = group.Count(),
                Completed = group.Count(assignment => assignment.Status == AssignmentStatus.Completed),
                RevenueThisMonth = group
                    .Where(assignment => assignment.AssignedOnUtc >= monthStart && assignment.AssignedOnUtc < nextMonthStart)
                    .Sum(assignment => (decimal?)assignment.PaidPrice) ?? 0m
            })
            .ToListAsync(ct);
        var salesProtocolIds = protocolSales.Select(sale => sale.ProtocolId).ToArray();
        var salesProtocols = salesProtocolIds.Length == 0
            ? []
            : await _db.Protocols.IgnoreQueryFilters()
                .AsNoTracking()
                .Where(protocol => salesProtocolIds.Contains(protocol.Id))
                .Select(protocol => new
                {
                    protocol.Id,
                    protocol.ProductFamilyId,
                    protocol.Title,
                    protocol.IsArchivedVersion,
                    protocol.CreatedAtUtc
                })
                .ToListAsync(ct);
        var protocolById = salesProtocols.ToDictionary(protocol => protocol.Id);
        var popularProtocols = protocolSales
            .Where(sale => protocolById.ContainsKey(sale.ProtocolId))
            .GroupBy(sale => protocolById[sale.ProtocolId].ProductFamilyId)
            .Select(group =>
            {
                var currentProtocol = group
                    .Select(sale => protocolById[sale.ProtocolId])
                    .OrderBy(protocol => protocol.IsArchivedVersion)
                    .ThenByDescending(protocol => protocol.CreatedAtUtc)
                    .First();
                var assignments = group.Sum(sale => sale.Assignments);
                var completed = group.Sum(sale => sale.Completed);
                return new PopularProtocolDto(
                    group.Key,
                    currentProtocol.Title,
                    assignments,
                    assignments == 0 ? 0 : Math.Round(completed / (double)assignments * 100, 1),
                    group.Sum(sale => sale.RevenueThisMonth));
            })
            .OrderByDescending(protocol => protocol.AthleteAssignments)
            .ThenByDescending(protocol => protocol.RevenueThisMonth)
            .Take(5)
            .ToList();

        var protocolRevenueThisMonth = monthAssignments.Sum(assignment => assignment.PaidPrice);
        return new AdminAnalyticsDashboardDto(
            new RevenueSummaryDto(totalGrossRevenue, currentDropInFee, protocolRevenueThisMonth, dropInRevenueThisMonth,
                monthAssignments.Count, thisMonthVisitorCheckIns),
            distribution,
            athleteGrowth,
            new AthleteVerificationStatsDto(
                verificationGroups.GetValueOrDefault(true),
                verificationGroups.GetValueOrDefault(false)),
            squadEngagement,
            todayVisits.Count,
            currentOccupancy,
            weeklyPeakTraffic,
            floorTraffic,
            machineUsageFrom,
            machineUsage,
            popularProtocols);
    }

    private DateTime ToUtc(DateOnly localDate)
    {
        var localMidnight = DateTime.SpecifyKind(localDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(localMidnight, _gymTimeZone);
    }

    private sealed class MachineUsageAccumulator(int id, string name)
    {
        public int Id { get; } = id;
        public string Name { get; } = name;
        public HashSet<int> SessionIds { get; } = [];
        public double EstimatedHours { get; set; }
        public Dictionary<int, int> SetHours { get; } = [];
    }
}
