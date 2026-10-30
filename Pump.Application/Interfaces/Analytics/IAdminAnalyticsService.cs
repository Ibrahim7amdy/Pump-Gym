using Pump.Application.Dtos.Analytics;

namespace Pump.Application.Interfaces.Analytics;

public interface IAdminAnalyticsService
{
    Task<AdminAnalyticsDashboardDto> GetDashboardAsync(CancellationToken ct = default);
}
