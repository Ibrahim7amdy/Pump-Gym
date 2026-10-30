using Pump.Domain.Common;

namespace Pump.Domain.Entities;

public class Notification : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public string Text { get; set; } = default!;
    public bool IsRead { get; set; } = false;
}