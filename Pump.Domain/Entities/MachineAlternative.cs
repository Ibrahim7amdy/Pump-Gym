namespace Pump.Domain.Entities;

public class MachineAlternative
{
    public int MachineId { get; set; }
    public Machine Machine { get; set; } = default!;

    public int AlternativeMachineId { get; set; }
    public Machine AlternativeMachine { get; set; } = default!;
}