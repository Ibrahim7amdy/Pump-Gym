using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;

namespace Pump.Infrastructure.Persistence.Configurations;

public class AthleteProtocolAssignmentConfiguration : IEntityTypeConfiguration<AthleteProtocolAssignment>
{
    public void Configure(EntityTypeBuilder<AthleteProtocolAssignment> builder)
    {
        builder.ToTable("AthleteProtocolAssignments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status).HasConversion<byte>();

        builder.Property(a => a.PaidPrice).HasColumnType("decimal(10,2)");

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Protocol)
            .WithMany()
            .HasForeignKey(a => a.ProtocolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AssignedByUser)
            .WithMany()
            .HasForeignKey(a => a.AssignedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // يوزر واحد يقدر يكون عنده Active assignment واحد بس في نفس الوقت
        builder.HasIndex(a => a.UserId)
            .IsUnique()
            .HasFilter("[Status] = 0");

        builder.HasQueryFilter(a => !a.User.IsDeleted);
    }
}