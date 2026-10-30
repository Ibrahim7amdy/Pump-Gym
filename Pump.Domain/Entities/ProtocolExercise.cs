namespace Pump.Domain.Entities;

public class ProtocolExercise
{
    public int Id { get; set; }

    public int ProtocolDayId { get; set; }
    public ProtocolDay ProtocolDay { get; set; } = default!;

    public int MachineId { get; set; }
    public Machine Machine { get; set; } = default!;

    public int Sets { get; set; }
    public string RepsLabel { get; set; } = default!;   // زي "8-10" أو "10-12"
    public decimal TargetWeightKg { get; set; }
    public int RestSeconds { get; set; }
    public int SortOrder { get; set; }
}