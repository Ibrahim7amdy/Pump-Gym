using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;

namespace Pump.Infrastructure.Persistence.Configurations;

public class ProtocolWeekPlanConfiguration : IEntityTypeConfiguration<ProtocolWeekPlan>
{
    public void Configure(EntityTypeBuilder<ProtocolWeekPlan> builder)
    {
        builder.ToTable("ProtocolWeekPlans");
        builder.HasKey(plan => plan.Id);
        builder.Property(plan => plan.PhaseLabel).HasMaxLength(80).IsRequired();
        builder.Property(plan => plan.LoadPercent).HasColumnType("decimal(5,2)");
        builder.HasIndex(plan => new { plan.ProtocolId, plan.WeekNumber }).IsUnique();
        builder.HasOne(plan => plan.Protocol)
            .WithMany(protocol => protocol.WeeklyLoadPlan)
            .HasForeignKey(plan => plan.ProtocolId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(plan => !plan.Protocol.IsDeleted);
    }
}
