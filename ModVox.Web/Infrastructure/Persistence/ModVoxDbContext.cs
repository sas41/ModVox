using Microsoft.EntityFrameworkCore;
using ModVox.Web.Domain;
using ModVox.Web.Infrastructure.Persistence.Configurations;

namespace ModVox.Web.Infrastructure.Persistence;

public sealed class ModVoxDbContext : DbContext
{
    public ModVoxDbContext(DbContextOptions<ModVoxDbContext> options) : base(options) { }

    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Mod> Mods => Set<Mod>();
    public DbSet<ModRelease> ModReleases => Set<ModRelease>();
    public DbSet<ModReleaseArtifact> ModReleaseArtifacts => Set<ModReleaseArtifact>();
    public DbSet<AccountSession> AccountSessions => Set<AccountSession>();
    public DbSet<ModReport> ModReports => Set<ModReport>();
    public DbSet<RefreshJob> RefreshJobs => Set<RefreshJob>();
    public DbSet<AuditLog> AuditLog => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserAccountConfiguration());
        modelBuilder.ApplyConfiguration(new GameConfiguration());
        modelBuilder.ApplyConfiguration(new TagConfiguration());
        modelBuilder.ApplyConfiguration(new ModConfiguration());
        modelBuilder.ApplyConfiguration(new ModReleaseConfiguration());
        modelBuilder.ApplyConfiguration(new ModReleaseArtifactConfiguration());
        modelBuilder.ApplyConfiguration(new AccountSessionConfiguration());
        modelBuilder.ApplyConfiguration(new ModReportConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshJobConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
    }
}
