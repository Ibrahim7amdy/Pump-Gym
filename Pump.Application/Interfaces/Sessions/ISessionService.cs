using Pump.Application.Dtos.Sessions;
using Pump.Domain.Entities;

namespace Pump.Application.Interfaces.Sessions
{
    public interface ISessionService
    {
        Task<SessionDto> StartAsync(int userId, CreateSessionRequestDto request, CancellationToken ct = default);
        Task<SessionDto> GetByIdAsync(int sessionId, int userId, CancellationToken ct = default);
        Task<SessionDto> GetCurrentAsync(int userId, CancellationToken ct = default);
        Task<WorkoutHistoryDto> GetHistoryAsync(int userId, int page, int pageSize, CancellationToken ct = default);
        Task<SessionSetDto> LogSetAsync(int sessionId, int userId, LogSetRequestDto request, CancellationToken ct = default);
        Task<SessionStationDto> AddStationAsync(int sessionId, int userId, AddStationRequestDto request, CancellationToken ct = default);
        Task<SessionStationDto> ReplaceStationMachineAsync(int sessionId, int stationId, int userId, ReplaceStationMachineRequestDto request, CancellationToken ct = default);
        Task<SessionDebriefDto> FinishAsync(int sessionId, int userId, CancellationToken ct = default);

        Task<int> CloseStaleSessionsAsync(CancellationToken ct = default);  
    }
}
