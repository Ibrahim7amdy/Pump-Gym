using Pump.Application.Dtos.Users;


namespace Pump.Application.Interfaces.Users;
public interface IAvatarPresetProvider
{
    IReadOnlyList<AvatarPresetDto> GetPresets();
}