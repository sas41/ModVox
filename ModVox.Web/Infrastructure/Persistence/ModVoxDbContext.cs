using Microsoft.EntityFrameworkCore;
using ModVox.Web.Domain;
using ModVox.Web.Infrastructure.Persistence.Configurations;

namespace ModVox.Web.Infrastructure.Persistence;

public sealed class ModVoxDbContext : DbContext
{
    public ModVoxDbContext(DbContextOptions<ModVoxDbContext> options) : base(options) { }

    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<GameRecord> Games => Set<GameRecord>();
    public DbSet<TagRecord> Tags => Set<TagRecord>();
    public DbSet<ModRecord> Mods => Set<ModRecord>();
    public DbSet<ModReleaseRecord> ModReleases => Set<ModReleaseRecord>();
    public DbSet<ModReleaseArtifactRecord> ModReleaseArtifacts => Set<ModReleaseArtifactRecord>();
    public DbSet<AccountSessionRecord> AccountSessions => Set<AccountSessionRecord>();
    public DbSet<ModReportRecord> ModReports => Set<ModReportRecord>();
    public DbSet<RefreshJobRecord> RefreshJobs => Set<RefreshJobRecord>();
    public DbSet<AuditLogRecord> AuditLog => Set<AuditLogRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserAccountConfiguration());
        modelBuilder.ApplyConfiguration(new GameRecordConfiguration());
        modelBuilder.ApplyConfiguration(new TagRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ModRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ModReleaseRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ModReleaseArtifactRecordConfiguration());
        modelBuilder.ApplyConfiguration(new AccountSessionRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ModReportRecordConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshJobRecordConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogRecordConfiguration());
    }
}
