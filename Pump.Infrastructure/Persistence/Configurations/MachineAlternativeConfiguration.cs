using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;

namespace Pump.Infrastructure.Persistence.Configurations;

public class MachineAlternativeConfiguration : IEntityTypeConfiguration<MachineAlternative>
{
    public void Configure(EntityTypeBuilder<MachineAlternative> builder)
    {
        builder.ToTable("MachineAlternatives");

        // Composite primary key for the many-to-many relationship
        builder.HasKey(ma => new { ma.MachineId, ma.AlternativeMachineId });

        builder.HasOne(ma => ma.Machine)
            .WithMany(m => m.Alternatives)
            .HasForeignKey(ma => ma.MachineId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ma => ma.AlternativeMachine)
            .WithMany()
            .HasForeignKey(ma => ma.AlternativeMachineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}