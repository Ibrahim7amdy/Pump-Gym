using Microsoft.AspNetCore.Http;
using Pump.Application.Dtos.Common;
using Pump.Application.Dtos.Machines;

namespace Pump.Api.Models.Machines;

public sealed class CreateMachineFormRequest
{
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
    public string? ImageUrl { get; set; }
    public string? SafetyNotes { get; set; }
    public List<string>? ExecutionSteps { get; set; }
    public int PinNumber { get; set; }
    public List<int>? AlternativeMachineIds { get; set; }

    public CreateMachineRequestDto ToDto(ImageUploadRequestDto? imageFile) => new(
        Name, MuscleGroup, Category, ImageUrl, SafetyNotes, ExecutionSteps, PinNumber,
        AlternativeMachineIds, imageFile);
}

public sealed class UpdateMachineFormRequest
{
    public string? Name { get; set; }
    public string? MuscleGroup { get; set; }
    public string? Category { get; set; }
    public IFormFile? Image { get; set; }
    public string? ImageUrl { get; set; }
    public string? SafetyNotes { get; set; }
    public List<string>? ExecutionSteps { get; set; }
    public int? PinNumber { get; set; }
    public List<int>? AlternativeMachineIds { get; set; }

    public UpdateMachineRequestDto ToDto(ImageUploadRequestDto? imageFile) => new(
        Name, MuscleGroup, Category, ImageUrl, SafetyNotes, ExecutionSteps, PinNumber,
        AlternativeMachineIds, imageFile);
}
