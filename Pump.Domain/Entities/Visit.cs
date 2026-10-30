using Pump.Domain.Common;
using Pump.Domain.Enums;

namespace Pump.Domain.Entities;

public class Visit : BaseEntity
{
    public int? UserId { get; set; }   // null if guest for walkin
    public User? User { get; set; }

    public string? GuestName { get; set; }   // only if walkin 

    public DateTime CheckInAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CheckOutAtUtc { get; set; }   // null if is still in the visit

    public VisitSource Source { get; set; }

    /// <summary>Fee captured for a one-time visitor at check-in; zero for registered athletes.</summary>
    public decimal DropInFee { get; set; }

    public int? CheckedInByStaffUserId { get; set; }   // only if Source = Admin
    public User? CheckedInByStaffUser { get; set; }

    public int? CheckedOutByStaffUserId { get; set; }  // admin who recorded the departure
    public User? CheckedOutByStaffUser { get; set; }
}
