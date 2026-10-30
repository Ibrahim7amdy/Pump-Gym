using Pump.Application.Dtos.Squads;

namespace Pump.Application.Interfaces.Squads;

public interface ISquadService
{
    Task<IReadOnlyList<SquadDto>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SquadDto>> GetForUserAsync(int userId, CancellationToken ct = default);
    Task<SquadDto> GetByIdAsync(int squadId, CancellationToken ct = default);
    Task<SquadDto> GetMemberViewAsync(int squadId, int userId, CancellationToken ct = default);
    Task<SquadDto> CreateAsync(int userId, CreateSquadRequestDto request, CancellationToken ct = default);
    Task<SquadDto> JoinAsync(int userId, string inviteCode, CancellationToken ct = default);
    Task<SquadDto> UpdateNameAsync(int squadId, int userId, string name, CancellationToken ct = default);
    Task DeleteAsync(int squadId, int userId, CancellationToken ct = default);
    Task<SquadDto> AddMemberAsync(int squadId, int callerId, AddSquadMemberRequestDto request, CancellationToken ct = default);
    Task RemoveMemberAsync(int squadId, int callerId, int userId, CancellationToken ct = default);
}
