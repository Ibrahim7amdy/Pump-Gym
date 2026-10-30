namespace Pump.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; } 
        public int UserId { get; set; }
        public User User { get; set; } = default!;
        public string TokenHash { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAtUtc { get; set; }
        public string? ReplacedByTokenHash { get; set; }
    }
}
