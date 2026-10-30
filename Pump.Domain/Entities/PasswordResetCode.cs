
namespace Pump.Domain.Entities;
public class PasswordResetCode
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public string CodeHash { get; set; } = default!;
    public string? ResetTokenHash { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAtUtc { get; set; }
    public bool IsUsed { get; set; } = false;
}