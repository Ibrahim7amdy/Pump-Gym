using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;

namespace Pump.Infrastructure.Persistence.Configurations;

public class ProtocolDayConfiguration : IEntityTypeConfiguration<ProtocolDay>
{
    public void Configure(EntityTypeBuilder<ProtocolDay> builder)
    {
        builder.ToTable("ProtocolDays");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Label).HasMaxLength(60).IsRequired();

        builder.Property(d => d.Intensity).HasConversion<byte>();

        builder.HasOne(d => d.Protocol)
            .WithMany(p => p.Days)
            .HasForeignKey(d => d.ProtocolId)
            .OnDelete(DeleteBehavior.Cascade);     // البروتوكول اتمسح؟ أيامه تتمسح معاه

        builder.HasIndex(d => new { d.ProtocolId, d.DayNumber })
            .IsUnique()
            .HasFilter("[DayNumber] IS NOT NULL"); 

        builder.HasQueryFilter(d => !d.Protocol.IsDeleted);

    }
}