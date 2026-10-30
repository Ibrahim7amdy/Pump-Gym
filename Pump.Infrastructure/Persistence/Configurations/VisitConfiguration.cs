using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;

namespace Pump.Infrastructure.Persistence.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("Visits");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.GuestName).HasMaxLength(120);

        builder.Property(v => v.Source).HasConversion<byte>();
        builder.Property(v => v.DropInFee).HasColumnType("decimal(10,2)");

        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict); // بيانات تاريخية، مينفعش تتمسح مع اليوزر

        builder.HasOne(v => v.CheckedInByStaffUser)
            .WithMany()
            .HasForeignKey(v => v.CheckedInByStaffUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.CheckedOutByStaffUser)
            .WithMany()
            .HasForeignKey(v => v.CheckedOutByStaffUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // filtered index يخلي استعلام "مين جوه الصالة دلوقتي" سريع جدًا
        builder.HasIndex(v => new { v.CheckInAtUtc, v.CheckOutAtUtc })
            .HasFilter("[CheckOutAtUtc] IS NULL");

        builder.HasIndex(v => v.UserId)
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL AND [CheckOutAtUtc] IS NULL");

        // الاتنين FK لـ User وnullable، فالفلتر هنا بيتأكد إن أي واحد منهم موجود يبقى مش محذوف
        builder.HasQueryFilter(v =>
            (v.User == null || !v.User.IsDeleted) &&
            (v.CheckedInByStaffUser == null || !v.CheckedInByStaffUser.IsDeleted));
    }
}
