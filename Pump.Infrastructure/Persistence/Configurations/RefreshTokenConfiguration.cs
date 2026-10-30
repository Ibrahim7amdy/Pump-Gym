using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;


namespace Pump.Infrastructure.Persistence.Configurations;
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasQueryFilter(rt => !rt.User.IsDeleted);

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash).IsRequired();
        builder.HasIndex(rt => rt.TokenHash).IsUnique();

        builder.Property(rt => rt.ReplacedByTokenHash).HasMaxLength(64);

        builder.HasIndex(rt => new { rt.UserId, rt.RevokedAtUtc });
    }
}