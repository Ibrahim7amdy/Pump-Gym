using Pump.Application.Dtos.Users;

namespace Pump.Application.Interfaces.Users;

public interface IAdminAthleteService
{
    Task<AdminAthletePageDto> GetAthletesAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<AdminAthleteDto> SetVerificationAsync(int athleteId, bool isVerified, CancellationToken ct = default);
    Task<AdminAthleteDto> UpdateCoachAsync(int athleteId, string? coachName, CancellationToken ct = default);
}
