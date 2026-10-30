using Microsoft.Extensions.Configuration;
using Pump.Application.Dtos.Users;
using Pump.Application.Interfaces.Users;


namespace Pump.Infrastructure.Services.Users;
public class AvatarPresetProvider : IAvatarPresetProvider
{
    private readonly List<AvatarPresetDto> _presets;

    public AvatarPresetProvider(IConfiguration config)
    {
        _presets = config.GetSection("AvatarPresets")
            .Get<List<AvatarPresetDto>>() ?? new();
    }

    public IReadOnlyList<AvatarPresetDto> GetPresets() => _presets;
}