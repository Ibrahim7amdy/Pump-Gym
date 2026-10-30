using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pump.Domain.Entities;

namespace Pump.Infrastructure.Persistence.Configurations;

public class SessionStationConfiguration : IEntityTypeConfiguration<SessionStation>
{
    public void Configure(EntityTypeBuilder<SessionStation> builder)
    {
        builder.ToTable("SessionStations");

        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.Session)
            .WithMany(se => se.Stations)
            .HasForeignKey(s => s.SessionId)
            .OnDelete(DeleteBehavior.Cascade); // الجلسة اتمسحت؟ محطاتها تتمسح معاها

        builder.HasOne(s => s.Machine)
            .WithMany()
            .HasForeignKey(s => s.MachineId)
            .OnDelete(DeleteBehavior.Restrict); // مينفعش تمسح ماكينة مستخدمة في تاريخ تمارين حقيقي

        builder.HasOne(ss => ss.ProtocolExercise)
       .WithMany()
       .HasForeignKey(ss => ss.ProtocolExerciseId)
       .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(ss => ss.SessionId);

        builder.HasQueryFilter(s => !s.Session.User.IsDeleted);

    }
}