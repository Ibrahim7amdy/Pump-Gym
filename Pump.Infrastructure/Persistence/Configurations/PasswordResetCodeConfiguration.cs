using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;

namespace Pump.Infrastructure.Persistence.Configurations;

public class PasswordResetCodeConfiguration : IEntityTypeConfiguration<PasswordResetCode>
{
    public void Configure(EntityTypeBuilder<PasswordResetCode> builder)
    {
        builder.ToTable("PasswordResetCodes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CodeHash).HasMaxLength(64).IsRequired();

        builder.Property(c => c.ResetTokenHash).HasMaxLength(64);

        builder.HasIndex(c => new { c.UserId, c.CodeHash });

        builder.HasIndex(c => c.ResetTokenHash);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(c => !c.User.IsDeleted);
    }
}