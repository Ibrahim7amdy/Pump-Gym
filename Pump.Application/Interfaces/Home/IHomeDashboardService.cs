using Pump.Application.Dtos.Home;

namespace Pump.Application.Interfaces.Home;

public interface IHomeDashboardService
{
    Task<HomeDashboardDto> GetAsync(int userId, CancellationToken ct = default);
}
