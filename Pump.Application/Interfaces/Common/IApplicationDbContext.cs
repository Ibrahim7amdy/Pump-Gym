using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pump.Domain.Entities;


namespace Pump.Application.Interfaces.Common
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<PasswordResetCode> PasswordResetCodes { get; }
        DbSet<Machine> Machines { get; }
        DbSet<MachineAlternative> MachineAlternatives { get; }
        DbSet<Protocol> Protocols { get; }
        DbSet<ProtocolDay> ProtocolDays { get; }
        DbSet<ProtocolExercise> ProtocolExercises { get; }
        DbSet<AthleteProtocolAssignment> AthleteProtocolAssignments { get; }
        DbSet<Session> Sessions { get; }
        DbSet<SessionStation> SessionStations { get; }
        DbSet<SessionSet> SessionSets { get; }
        DbSet<Squad> Squads { get; }
        DbSet<SquadMember> SquadMembers { get; }
        DbSet<Visit> Visits { get; }
        DbSet<GymPricingSetting> GymPricingSettings { get; }
        DbSet<AnalyticsDailySnapshot> AnalyticsDailySnapshots { get; }
        DbSet<Notification> Notifications { get; }
        ChangeTracker ChangeTracker { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
