
namespace Pump.Application.Dtos.Machines
{
    public record CreateMachineRequestDto(
    string Name,
    string MuscleGroup,
    string Category,
    string? ImageUrl,
    string? SafetyNotes,
    List<string>? ExecutionSteps,
    int PinNumber,
    List<int>? AlternativeMachineIds,
    Pump.Application.Dtos.Common.ImageUploadRequestDto? ImageFile = null);
}
