using Pump.Application.Dtos.Protocols;


namespace Pump.Application.Interfaces.Protocols;
public interface IProtocolService
{
    Task<List<ProtocolListItemDto>> GetAllAsync(CancellationToken ct = default);
    Task<ProtocolDetailDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<ProtocolAchievementDto>> GetCompletedAsync(int userId, CancellationToken ct = default);
    Task<ProtocolDetailDto> CreateAsync(int createdByAdminUserId, CreateProtocolRequestDto request, Stream? imageStream, string? imageContentType, CancellationToken ct = default);
    Task<ProtocolDetailDto> UpdateAsync(int id, UpdateProtocolRequestDto request, Stream? imageStream, string? imageContentType, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task AssignAsync(int protocolId, int assignedByUserId, AssignProtocolRequestDto request, CancellationToken ct = default);
    Task<ActiveProtocolDto> GetActiveAsync(int userId, CancellationToken ct = default);
}
