using Pump.Domain.Common;
using Pump.Domain.Enums;

namespace Pump.Domain.Entities;

public class Protocol : SoftDeletableEntity
{
    public Guid ProductFamilyId { get; set; } = Guid.NewGuid();
    public bool IsArchivedVersion { get; set; }

    public string Title { get; set; } = default!;
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public int? CreatedByAdminUserId { get; set; }   // Admin is also the coach 
    public User? CreatedByAdminUser { get; set; }

    public string Category { get; set; } = default!;
    public ProtocolDifficulty Difficulty { get; set; }
    public string? ExpectedGainLabel { get; set; }
    public string? ExpectedMuscleGainLabel { get; set; }
    public string? ExpectedStrengthGainLabel { get; set; }
    public int? ExpectedPoints { get; set; }
    public string? ImageUrl { get; set; }

    public ICollection<ProtocolDay> Days { get; set; } = new List<ProtocolDay>();
    public ICollection<ProtocolWeekPlan> WeeklyLoadPlan { get; set; } = new List<ProtocolWeekPlan>();
}
