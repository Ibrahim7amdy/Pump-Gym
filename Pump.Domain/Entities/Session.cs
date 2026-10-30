using Pump.Domain.Common;

namespace Pump.Domain.Entities;

public class Session : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int? ProtocolDayId { get; set; }   // is null if ad-hoc session 
    public ProtocolDay? ProtocolDay { get; set; }

    public int? AthleteProtocolAssignmentId  { get; set; }           
    public AthleteProtocolAssignment? AthleteProtocolAssignment { get; set; }

    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAtUtc { get; set; }
    public decimal? TotalVolumeKg { get; set; }   // Computed when the session ends, based on the sum of all station volumes
    public int? CaloriesEstimate { get; set; }     // Computed when the session ends, based on the sum of all station calories
    public double? IntensityPercent { get; set; }
    public int? XpAwarded { get; set; }
    public byte[] RowVersion { get; set; } = default!;
    public ICollection<SessionStation> Stations { get; set; } = new List<SessionStation>();
}
