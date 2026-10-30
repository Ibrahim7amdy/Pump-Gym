namespace Pump.Application.Dtos.Protocols;

public record CreateProtocolWeekPlanDto(int WeekNumber, string PhaseLabel, decimal LoadPercent);
