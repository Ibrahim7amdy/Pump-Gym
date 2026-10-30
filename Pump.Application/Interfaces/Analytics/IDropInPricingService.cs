using Pump.Application.Dtos.Analytics;

namespace Pump.Application.Interfaces.Analytics;

public interface IDropInPricingService
{
    Task<DropInPricingDto> GetAsync(CancellationToken ct = default);
    Task<DropInPricingDto> UpdateAsync(DropInPricingDto request, CancellationToken ct = default);
}
