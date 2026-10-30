using Pump.Domain.Common;
using Pump.Domain.Enums;

namespace Pump.Domain.Entities
{
    public class User : SoftDeletableEntity
    {
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public UserRole Role { get; set; } = UserRole.User;

        public string? RankTitle { get; set; }
        public string? CoachName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public decimal? TargetWeightKg { get; set; }
        public decimal? CurrentWeightKg { get; set; }
        public decimal? HeightCm { get; set; }
        public int? CalorieGoal { get; set; }
        public decimal? WaterGoalLiters { get; set; }
        public int Xp { get; set; } = 0;
        public string PreferredLanguage { get; set; } = "en";
        public PumpThemeId PreferredThemeId { get; set; } = PumpThemeId.Lime;
        public bool IsVerified { get; set; } = false;


        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
