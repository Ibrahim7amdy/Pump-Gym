namespace Pump.Domain.Entities;

public class ProtocolWeekPlan
{
    public int Id { get; set; }
    public int ProtocolId { get; set; }
    public Protocol Protocol { get; set; } = default!;
    public int WeekNumber { get; set; }
    public string PhaseLabel { get; set; } = default!;
    public decimal LoadPercent { get; set; }
}
