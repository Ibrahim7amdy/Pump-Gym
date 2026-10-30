using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;

namespace Pump.Infrastructure.Persistence.Configurations;

public sealed class GymPricingSettingConfiguration : IEntityTypeConfiguration<GymPricingSetting>
{
    public void Configure(EntityTypeBuilder<GymPricingSetting> builder)
    {
        builder.ToTable("GymPricingSettings", table => table.HasCheckConstraint(
            "CK_GymPricingSettings_DropInFee_NonNegative", "[DropInFee] >= 0"));
        builder.HasKey(setting => setting.Id);
        builder.Property(setting => setting.Id).ValueGeneratedNever();
        builder.Property(setting => setting.DropInFee).HasColumnType("decimal(10,2)");
    }
}
