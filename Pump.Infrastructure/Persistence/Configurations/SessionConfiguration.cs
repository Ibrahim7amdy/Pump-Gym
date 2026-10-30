using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;


namespace Pump.Infrastructure.Persistence.Configurations;
public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Sessions");

        builder.Property(s => s.TotalVolumeKg).HasColumnType("decimal(10,2)");
        builder.Property(s => s.RowVersion).IsRowVersion();

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict); // بيانات تاريخية، مينفعش تتمسح مع اليوزر

        builder.HasOne(s => s.ProtocolDay)
            .WithMany()
            .HasForeignKey(s => s.ProtocolDayId)
            .OnDelete(DeleteBehavior.Restrict); // البروتوكول اتمسح؟ الجلسة تفضل موجودة كـ "جلسة حرة"

        builder.HasOne(s => s.AthleteProtocolAssignment)
             .WithMany()
             .HasForeignKey(s => s.AthleteProtocolAssignmentId)
             .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.AthleteProtocolAssignmentId);
        builder.HasIndex(s => s.UserId)
            .IsUnique()
            .HasFilter("[EndedAtUtc] IS NULL");

        builder.Property(s => s.IntensityPercent).HasColumnType("float");

        builder.HasQueryFilter(s => !s.User.IsDeleted);
    }
}
