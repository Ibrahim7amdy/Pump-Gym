using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;


namespace Pump.Infrastructure.Persistence.Configurations;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasQueryFilter(u => !u.IsDeleted);

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username).HasMaxLength(30).IsRequired();
        builder.HasIndex(u => u.Username).IsUnique();

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.FullName).HasMaxLength(120).IsRequired();

        builder.Property(u => u.PasswordHash).IsRequired();

        builder.Property(u => u.RankTitle).HasMaxLength(60);
        builder.Property(u => u.CoachName).HasMaxLength(120);

        builder.Property(u => u.Bio).HasMaxLength(280);

        builder.Property(u => u.AvatarUrl).HasMaxLength(500);

        builder.Property(u => u.TargetWeightKg).HasColumnType("decimal(5,2)");

        builder.Property(u => u.CurrentWeightKg).HasColumnType("decimal(5,2)");

        builder.Property(u => u.HeightCm).HasColumnType("decimal(5,1)");

        builder.Property(u => u.WaterGoalLiters).HasColumnType("decimal(4,1)");

        builder.Property(u => u.Role).HasConversion<byte>();                               // tinyint in the database

        builder.Property(u => u.PreferredLanguage).HasMaxLength(2).HasDefaultValue("en");

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
