using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;
using System.Text.Json;

namespace Pump.Infrastructure.Persistence.Configurations;

public class MachineConfiguration : IEntityTypeConfiguration<Machine>
{
    public void Configure(EntityTypeBuilder<Machine> builder)
    {
        builder.ToTable("Machines");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).HasMaxLength(120).IsRequired();

        builder.Property(m => m.MuscleGroup).HasMaxLength(50).IsRequired();

        builder.Property(m => m.Category).HasMaxLength(50).IsRequired();

        builder.Property(m => m.ImageUrl).HasMaxLength(500).IsRequired();

        builder.Property(m => m.ExecutionSteps)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),                                      // إزاي يقارن قيمتين
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),    // إزاي يحسب الـ Hash
                c => c.ToList()));                                         

        builder.HasIndex(m => m.PinNumber).IsUnique();
    }
}