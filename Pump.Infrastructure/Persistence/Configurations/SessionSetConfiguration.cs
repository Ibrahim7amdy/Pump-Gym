using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;

namespace Pump.Infrastructure.Persistence.Configurations;

public class SessionSetConfiguration : IEntityTypeConfiguration<SessionSet>
{
    public void Configure(EntityTypeBuilder<SessionSet> builder)
    {
        builder.ToTable("SessionSets");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.WeightKg).HasColumnType("decimal(6,2)");

        builder.HasOne(s => s.SessionStation)
            .WithMany(st => st.Sets)
            .HasForeignKey(s => s.SessionStationId)
            .OnDelete(DeleteBehavior.Cascade);


        builder.HasIndex(s => new { s.SessionStationId, s.SetNumber }).IsUnique();
        builder.HasIndex(s => new { s.SessionStationId, s.WeightKg });

        builder.HasIndex(s => s.LoggedAtUtc);

        builder.HasQueryFilter(s => !s.SessionStation.Session.User.IsDeleted);

    }
}
