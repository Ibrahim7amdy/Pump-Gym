namespace Pump.Domain.Entities;

public class SessionSet
{
    public int Id { get; set; }

    public int SessionStationId { get; set; }
    public SessionStation SessionStation { get; set; } = default!;

    public int SetNumber { get; set; }
    public decimal WeightKg { get; set; }
    public int Reps { get; set; }
    public DateTime LoggedAtUtc { get; set; } = DateTime.UtcNow;
}