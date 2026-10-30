using Pump.Application.Dtos.Users;

namespace Pump.Application.Interfaces.Users;

public interface IUserProfileService
{
    Task<UserDto> GetMeAsync(int userId, CancellationToken ct = default);
    Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto request, CancellationToken ct = default);
    Task<UserDto> SetAvatarUrlAsync(int userId, SetAvatarUrlRequestDto request, CancellationToken ct = default);
    Task<UserDto> UploadAvatarAsync(int userId, Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    Task<UserDto> UpdatePreferencesAsync(int userId, UpdatePreferencesRequestDto request, CancellationToken ct = default);
    IReadOnlyList<AvatarPresetDto> GetAvatarPresets();
}