using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModVox.Web.Domain;

namespace ModVox.Web.Infrastructure.Persistence.Configurations;

public sealed class ModReleaseRecordConfiguration : IEntityTypeConfiguration<ModReleaseRecord>
{
    public void Configure(EntityTypeBuilder<ModReleaseRecord> builder)
    {
        builder.HasIndex(r => r.ModId).HasDatabaseName("ix_mod_releases_mod_id");
        builder.HasIndex(r => new { r.ModId, r.TagName }).IsUnique().HasDatabaseName("ix_mod_releases_mod_tag");

        builder.HasMany(r => r.Artifacts)
            .WithOne(a => a.Release)
            .HasForeignKey(a => a.ReleaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
