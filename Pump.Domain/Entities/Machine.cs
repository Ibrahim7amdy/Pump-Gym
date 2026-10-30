using Pump.Domain.Common;

namespace Pump.Domain.Entities;

public class Machine : BaseEntity
{
    public string Name { get; set; } = default!;
    public string MuscleGroup { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string ImageUrl { get; set; } = default!;
    public int PinNumber { get; set; }
    public string? SafetyNotes { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> ExecutionSteps { get; set; } = new();

    public ICollection<MachineAlternative> Alternatives { get; set; } = new List<MachineAlternative>();
}