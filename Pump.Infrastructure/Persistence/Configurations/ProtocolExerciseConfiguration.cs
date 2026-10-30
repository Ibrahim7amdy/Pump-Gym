using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;

namespace Pump.Infrastructure.Persistence.Configurations;

public class ProtocolExerciseConfiguration : IEntityTypeConfiguration<ProtocolExercise>
{
    public void Configure(EntityTypeBuilder<ProtocolExercise> builder)
    {
        builder.ToTable("ProtocolExercises");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.RepsLabel).HasMaxLength(20).IsRequired();

        builder.Property(e => e.TargetWeightKg).HasColumnType("decimal(6,2)");

        builder.HasOne(e => e.ProtocolDay)
            .WithMany(d => d.Exercises)
            .HasForeignKey(e => e.ProtocolDayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Machine)
            .WithMany()
            .HasForeignKey(e => e.MachineId)
            .OnDelete(DeleteBehavior.Restrict); // مينفعش تمسح ماكينة مستخدمة جوه بروتوكول

        builder.HasQueryFilter(a => !a.ProtocolDay.Protocol.IsDeleted);

    }
}