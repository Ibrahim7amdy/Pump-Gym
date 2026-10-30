using Microsoft.EntityFrameworkCore;
using Pump.Application.Dtos.Analytics;
using Pump.Application.Interfaces.Analytics;
using Pump.Application.Interfaces.Common;

namespace Pump.Application.Services.Analytics;

public sealed class DropInPricingService : IDropInPricingService
{
    private const int SingletonId = 1;
    private readonly IApplicationDbContext _db;

    public DropInPricingService(IApplicationDbContext db) => _db = db;

    public async Task<DropInPricingDto> GetAsync(CancellationToken ct = default)
    {
        var fee = await _db.GymPricingSettings.AsNoTracking()
            .Where(setting => setting.Id == SingletonId)
            .Select(setting => (decimal?)setting.DropInFee)
            .SingleOrDefaultAsync(ct);
        return new DropInPricingDto(fee);
    }

    public async Task<DropInPricingDto> UpdateAsync(DropInPricingDto request, CancellationToken ct = default)
    {
        var setting = await _db.GymPricingSettings
            .SingleOrDefaultAsync(item => item.Id == SingletonId, ct);
        if (setting is null)
        {
            setting = new Pump.Domain.Entities.GymPricingSetting { Id = SingletonId };
            _db.GymPricingSettings.Add(setting);
        }

        if (request.DropInFee is null)
            throw new Pump.Application.Common.Exceptions.ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.DropInFee)] = ["Drop-in fee is required."]
            });

        setting.DropInFee = request.DropInFee.Value;
        await _db.SaveChangesAsync(ct);
        return new DropInPricingDto(setting.DropInFee);
    }
}
