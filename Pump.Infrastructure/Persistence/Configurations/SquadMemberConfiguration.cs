using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;

namespace Pump.Infrastructure.Persistence.Configurations;

public class SquadMemberConfiguration : IEntityTypeConfiguration<SquadMember>
{
    public void Configure(EntityTypeBuilder<SquadMember> builder)
    {
        builder.ToTable("SquadMembers");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.TonnageContributed).HasColumnType("decimal(10,2)");

        builder.HasOne(m => m.Squad)
            .WithMany(s => s.Members)
            .HasForeignKey(m => m.SquadId)
            .OnDelete(DeleteBehavior.Cascade); // المجموعة اتمسحت؟ عضويتها تتمسح معاها

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        //  unique index on (SquadId, UserId))
        builder.HasIndex(m => new { m.SquadId, m.UserId }).IsUnique();

        builder.HasQueryFilter(m => !m.Squad.IsDeleted && !m.User.IsDeleted);
    }
}