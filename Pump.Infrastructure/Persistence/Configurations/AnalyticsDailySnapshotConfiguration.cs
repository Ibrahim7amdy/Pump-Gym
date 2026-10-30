using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;

namespace Pump.Infrastructure.Persistence.Configurations;

public class AnalyticsDailySnapshotConfiguration : IEntityTypeConfiguration<AnalyticsDailySnapshot>
{
    public void Configure(EntityTypeBuilder<AnalyticsDailySnapshot> builder)
    {
        builder.ToTable("AnalyticsDailySnapshots");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TotalRevenue).HasColumnType("decimal(12,2)");

        builder.Property(a => a.AvgOccupancy).HasColumnType("decimal(5,2)");

        // يوم واحد بس له صف واحد
        builder.HasIndex(a => a.SnapshotDateUtc).IsUnique();
    }
}