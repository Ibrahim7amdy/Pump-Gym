using Pump.Application.Dtos.Machines;

namespace Pump.Application.Interfaces.Machines;

public interface IMachineService
{
    Task<List<MachineDto>> GetAllAsync(string? muscleGroup, int? currentUserId, CancellationToken ct = default);
    Task<MachineDto> GetByIdAsync(int id, int? currentUserId, CancellationToken ct = default);
    Task<MachineDto> CreateAsync(CreateMachineRequestDto request, Stream? imageStream, string? imageContentType, CancellationToken ct = default);
    Task<MachineDto> UpdateAsync(int id, UpdateMachineRequestDto request, Stream? imageStream, string? imageContentType, CancellationToken ct = default);
    Task<bool> ToggleActiveAsync(int id, CancellationToken ct = default);
}
