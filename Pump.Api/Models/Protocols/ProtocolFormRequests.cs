using Microsoft.AspNetCore.Http;
using Pump.Application.Dtos.Protocols;

namespace Pump.Api.Models.Protocols;

public sealed class CreateProtocolFormRequest
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public string? ExpectedGainLabel { get; set; }
    public string? ExpectedMuscleGainLabel { get; set; }
    public string? ExpectedStrengthGainLabel { get; set; }
    public int? ExpectedPoints { get; set; }
    public IFormFile? Image { get; set; }
    public List<ProtocolDayFormRequest> Days { get; set; } = [];
    public List<ProtocolWeekPlanFormRequest> WeeklyLoadPlan { get; set; } = [];

    public CreateProtocolRequestDto ToDto() => new(
        Title, Category, Difficulty, Price, DurationDays, ExpectedGainLabel,
        ExpectedMuscleGainLabel, ExpectedStrengthGainLabel, ExpectedPoints, null,
        Days.Select(day => day.ToDto()).ToList(),
        WeeklyLoadPlan.Select(week => week.ToDto()).ToList());
}

public sealed class UpdateProtocolFormRequest
{
    public string? Title { get; set; }
    public string? Category { get; set; }
    public string? Difficulty { get; set; }
    public decimal? Price { get; set; }
    public int? DurationDays { get; set; }
    public string? ExpectedGainLabel { get; set; }
    public string? ExpectedMuscleGainLabel { get; set; }
    public string? ExpectedStrengthGainLabel { get; set; }
    public int? ExpectedPoints { get; set; }
    public IFormFile? Image { get; set; }
    public List<ProtocolDayFormRequest>? Days { get; set; }
    public List<ProtocolWeekPlanFormRequest>? WeeklyLoadPlan { get; set; }

    public UpdateProtocolRequestDto ToDto() => new(
        Title, Category, Difficulty, Price, DurationDays, ExpectedGainLabel,
        ExpectedMuscleGainLabel, ExpectedStrengthGainLabel, ExpectedPoints, null,
        Days?.Select(day => day.ToDto()).ToList(),
        WeeklyLoadPlan?.Select(week => week.ToDto()).ToList());
}

public sealed class ProtocolWeekPlanFormRequest
{
    public int WeekNumber { get; set; }
    public string PhaseLabel { get; set; } = string.Empty;
    public decimal LoadPercent { get; set; }

    public CreateProtocolWeekPlanDto ToDto() => new(WeekNumber, PhaseLabel, LoadPercent);
}

public sealed class ProtocolDayFormRequest
{
    public int? DayNumber { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Intensity { get; set; } = string.Empty;
    public List<ProtocolExerciseFormRequest> Exercises { get; set; } = [];

    public CreateProtocolDayRequestDto ToDto() => new(
        DayNumber, Label, Intensity, Exercises.Select(exercise => exercise.ToDto()).ToList());
}

public sealed class ProtocolExerciseFormRequest
{
    public int MachineId { get; set; }
    public int Sets { get; set; }
    public string RepsLabel { get; set; } = string.Empty;
    public decimal TargetWeightKg { get; set; }
    public int RestSeconds { get; set; }
    public int SortOrder { get; set; }

    public CreateProtocolExerciseRequestDto ToDto() => new(
        MachineId, Sets, RepsLabel, TargetWeightKg, RestSeconds, SortOrder);
}
