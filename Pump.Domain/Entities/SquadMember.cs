using Pump.Domain.Common;


namespace Pump.Domain.Entities;
public class SquadMember : BaseEntity
{
    public int SquadId { get; set; }
    public Squad Squad { get; set; } = default!;

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public decimal TonnageContributed { get; set; } = 0;
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
}