using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;

namespace Pump.Infrastructure.Persistence.Configurations;

public class SquadConfiguration : IEntityTypeConfiguration<Squad>
{
    public void Configure(EntityTypeBuilder<Squad> builder)
    {
        builder.ToTable("Squads");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(60).IsRequired();

        builder.Property(s => s.InviteCode).HasMaxLength(12).IsRequired();

        builder.Property(s => s.MonthlyGoalTonnage).HasColumnType("decimal(10,2)");

        builder.HasIndex(s => s.InviteCode).IsUnique();

        builder.HasOne(s => s.CreatedByUser)
            .WithMany()
            .HasForeignKey(s => s.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(s => !s.IsDeleted && !s.CreatedByUser.IsDeleted);
    }
}