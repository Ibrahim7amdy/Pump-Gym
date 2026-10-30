namespace Pump.Application.Dtos.Analytics;

public sealed record AdminAnalyticsDashboardDto(
    RevenueSummaryDto Revenue,
    IReadOnlyList<RevenueDistributionDto> RevenueDistribution,
    IReadOnlyList<AthleteGrowthMonthDto> AthleteGrowth,
    AthleteVerificationStatsDto AthleteVerification,
    IReadOnlyList<SquadEngagementDto> SquadEngagement,
    int TodayGymCheckIns,
    int CurrentOccupancy,
    IReadOnlyList<DailyTrafficDto> WeeklyPeakTraffic,
    IReadOnlyList<HourlyTrafficDto> FloorTraffic,
    DateOnly MachineUsageFrom,
    IReadOnlyList<MachineUsageDto> MachineUsage,
    IReadOnlyList<PopularProtocolDto> PopularProtocols);

public sealed record RevenueSummaryDto(
    decimal GrossRevenue,
    decimal? CurrentDropInFee,
    decimal ProtocolRevenueThisMonth,
    decimal DropInRevenueThisMonth,
    int ProtocolAssignmentsThisMonth,
    int OneTimeVisitorCheckInsThisMonth);

public sealed record RevenueDistributionDto(string Category, decimal Revenue, double Percent);

public sealed record AthleteGrowthMonthDto(string Month, int NewAthletes);

public sealed record AthleteVerificationStatsDto(int Verified, int Unverified);

public sealed record SquadEngagementDto(int SquadId, string SquadName, decimal MonthlyTonnage);

public sealed record DailyTrafficDto(string Day, int CheckIns);

public sealed record HourlyTrafficDto(string Hour, decimal AverageOccupancy);

public sealed record MachineUsageDto(
    int MachineId,
    string MachineName,
    int Sessions,
    decimal EstimatedHours,
    double EstimatedUtilizationPercent,
    string? PeakTime);

public sealed record PopularProtocolDto(
    Guid ProductFamilyId,
    string Title,
    int AthleteAssignments,
    double CompletionRatePercent,
    decimal RevenueThisMonth);
