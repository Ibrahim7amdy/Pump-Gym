using Pump.Domain.Common;

namespace Pump.Domain.Entities;

public class Squad : SoftDeletableEntity
{
    public string Name { get; set; } = default!;
    public string InviteCode { get; set; } = default!;
    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = default!;
    public decimal MonthlyGoalTonnage { get; set; }

    public ICollection<SquadMember> Members { get; set; } = new List<SquadMember>();
}