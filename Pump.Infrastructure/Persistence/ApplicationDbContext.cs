using Microsoft.EntityFrameworkCore;
using Pump.Application.Interfaces.Common;
using Pump.Domain.Common;
using Pump.Domain.Entities;



namespace Pump.Infrastructure.Persistence;
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetCode> PasswordResetCodes => Set<PasswordResetCode>();
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<MachineAlternative> MachineAlternatives => Set<MachineAlternative>();
    public DbSet<Protocol> Protocols => Set<Protocol>();
    public DbSet<ProtocolDay> ProtocolDays => Set<ProtocolDay>();
    public DbSet<ProtocolExercise> ProtocolExercises => Set<ProtocolExercise>();
    public DbSet<AthleteProtocolAssignment> AthleteProtocolAssignments => Set<AthleteProtocolAssignment>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SessionStation> SessionStations => Set<SessionStation>();
    public DbSet<SessionSet> SessionSets => Set<SessionSet>();
    public DbSet<Squad> Squads => Set<Squad>();
    public DbSet<SquadMember> SquadMembers => Set<SquadMember>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<GymPricingSetting> GymPricingSettings => Set<GymPricingSetting>();
    public DbSet<AnalyticsDailySnapshot> AnalyticsDailySnapshots => Set<AnalyticsDailySnapshot>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(builder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
