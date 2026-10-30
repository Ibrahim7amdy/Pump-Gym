using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;

namespace Pump.Infrastructure.Persistence.Configurations;

public class ProtocolConfiguration : IEntityTypeConfiguration<Protocol>
{
    public void Configure(EntityTypeBuilder<Protocol> builder)
    {
        builder.ToTable("Protocols");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProductFamilyId)
            .HasDefaultValueSql("NEWID()");

        builder.HasIndex(p => p.ProductFamilyId);

        builder.Property(p => p.IsArchivedVersion)
            .HasDefaultValue(false);

        builder.Property(p => p.Title).HasMaxLength(150).IsRequired();

        builder.Property(p => p.Category).HasMaxLength(50).IsRequired();

        builder.Property(p => p.Difficulty).HasConversion<byte>();

        builder.Property(p => p.Price).HasColumnType("decimal(10,2)");

        builder.Property(p => p.ExpectedGainLabel).HasMaxLength(120);
        builder.Property(p => p.ExpectedMuscleGainLabel).HasMaxLength(120);
        builder.Property(p => p.ExpectedStrengthGainLabel).HasMaxLength(120);

        builder.Property(p => p.ImageUrl).HasMaxLength(500);

        builder.HasOne(p => p.CreatedByAdminUser)
            .WithMany()
            .HasForeignKey(p => p.CreatedByAdminUserId)
            .OnDelete(DeleteBehavior.SetNull);   // لو الأدمن اتمسح، البروتوكول يفضل موجود من غير "مدرب"

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
