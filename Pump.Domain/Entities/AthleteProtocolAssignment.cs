using Pump.Domain.Common;
using Pump.Domain.Enums;

namespace Pump.Domain.Entities;

public class AthleteProtocolAssignment : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int ProtocolId { get; set; }
    public Protocol Protocol { get; set; } = default!;

    public int? AssignedByUserId { get; set; }
    public User? AssignedByUser { get; set; }

    public DateOnly AssignedOnUtc { get; set; }
    public DateOnly ExpiresOnUtc { get; set; }
    public decimal PaidPrice { get; set; }
    public int CompletedDaysCount { get; set; } = 0;
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Active;
}