using Pump.Application.Dtos.Users;

namespace Pump.Application.Interfaces.Users;

public interface ILeaderboardService
{
    Task<IReadOnlyList<LeaderboardItemDto>> GetAsync(
        string? tier,
        string timeframe,
        CancellationToken ct = default);
}
