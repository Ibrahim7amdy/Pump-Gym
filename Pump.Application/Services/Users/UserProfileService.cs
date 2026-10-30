using Microsoft.EntityFrameworkCore;
using Pump.Application.Common.Exceptions;
using Pump.Application.Dtos.Users;
using Pump.Application.Interfaces.Common;
using Pump.Application.Interfaces.Users;
using Pump.Domain.Entities;
using Pump.Domain.Enums;


namespace Pump.Application.Services.Users;
public class UserProfileService : IUserProfileService
{
    private readonly IApplicationDbContext _db;
    private readonly IFileStorageService _fileStorage;
    private readonly IAvatarPresetProvider _presetProvider;

    public UserProfileService(IApplicationDbContext db, IFileStorageService fileStorage, IAvatarPresetProvider presetProvider)
    {
        _db = db;
        _fileStorage = fileStorage;
        _presetProvider = presetProvider;
    }

    public async Task<UserDto> GetMeAsync(int userId, CancellationToken ct = default)
        => ToDto(await GetUserOrThrowAsync(userId, ct));

    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto request, CancellationToken ct = default)
    {
        var user = await GetUserOrThrowAsync(userId, ct);

        if (request.FullName is not null) user.FullName = request.FullName;
        if (request.RankTitle is not null) user.RankTitle = request.RankTitle;
        if (request.Bio is not null) user.Bio = request.Bio;
        if (request.TargetWeightKg is not null) user.TargetWeightKg = request.TargetWeightKg;
        if (request.CurrentWeightKg is not null) user.CurrentWeightKg = request.CurrentWeightKg;
        if (request.HeightCm is not null) user.HeightCm = request.HeightCm;
        if (request.CalorieGoal is not null) user.CalorieGoal = request.CalorieGoal;
        if (request.WaterGoalLiters is not null) user.WaterGoalLiters = request.WaterGoalLiters;

        await _db.SaveChangesAsync(ct);
        return ToDto(user);
    }

    public async Task<UserDto> SetAvatarUrlAsync(int userId, SetAvatarUrlRequestDto request, CancellationToken ct = default)
    {
        var user = await GetUserOrThrowAsync(userId, ct);
        user.AvatarUrl = request.AvatarUrl;
        await _db.SaveChangesAsync(ct);
        return ToDto(user);
    }

    public async Task<UserDto> UploadAvatarAsync(int userId, Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        var user = await GetUserOrThrowAsync(userId, ct);
        user.AvatarUrl = await _fileStorage.SaveAsync(fileStream, fileName, contentType, ct);
        await _db.SaveChangesAsync(ct);
        return ToDto(user);
    }

    public async Task<UserDto> UpdatePreferencesAsync(int userId, UpdatePreferencesRequestDto request, CancellationToken ct = default)
    {
        var user = await GetUserOrThrowAsync(userId, ct);

        if (request.PreferredLanguage is not null)
            user.PreferredLanguage = request.PreferredLanguage.ToLowerInvariant();

        if (request.PreferredThemeId is not null)
            user.PreferredThemeId = Enum.Parse<PumpThemeId>(request.PreferredThemeId, ignoreCase: true);

        await _db.SaveChangesAsync(ct);
        return ToDto(user);
    }

    public IReadOnlyList<AvatarPresetDto> GetAvatarPresets() => _presetProvider.GetPresets();

    private async Task<User> GetUserOrThrowAsync(int userId, CancellationToken ct)
        => await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException("User not found.");

    private static UserDto ToDto(User user) => new(
        user.Id, user.Username, user.Email, user.FullName, user.Role.ToString(),
        user.RankTitle, user.CoachName, user.Bio, user.AvatarUrl,
        user.TargetWeightKg, user.CurrentWeightKg, user.HeightCm, user.CalorieGoal, user.WaterGoalLiters,
        user.Xp, user.PreferredLanguage,
        user.PreferredThemeId.ToString().ToLowerInvariant(),
        user.IsVerified,
        user.CreatedAtUtc);
}
