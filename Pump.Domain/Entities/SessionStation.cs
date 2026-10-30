namespace Pump.Domain.Entities;

public class SessionStation
{
    public int Id { get; set; }

    public int SessionId { get; set; }
    public Session Session { get; set; } = default!;

    public int MachineId { get; set; }
    public Machine Machine { get; set; } = default!;

    public int? ProtocolExerciseId { get; set; }
    public ProtocolExercise? ProtocolExercise { get; set; }

    public int TargetSets { get; set; }
    public int RestSeconds { get; set; }
    public int SortOrder { get; set; }

    public ICollection<SessionSet> Sets { get; set; } = new List<SessionSet>();
}