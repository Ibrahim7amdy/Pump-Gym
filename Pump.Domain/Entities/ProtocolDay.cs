using Pump.Domain.Enums;

namespace Pump.Domain.Entities;

public class ProtocolDay
{
    public int Id { get; set; }

    public int ProtocolId { get; set; }
    public Protocol Protocol { get; set; } = default!;

    public int? DayNumber { get; set; }   // nullable to allow free plan
    public string Label { get; set; } = default!;
    public ProtocolDayIntensity Intensity { get; set; }

    public ICollection<ProtocolExercise> Exercises { get; set; } = new List<ProtocolExercise>();
}