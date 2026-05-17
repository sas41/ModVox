using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModVox.Web.Domain;

namespace ModVox.Web.Infrastructure.Persistence.Configurations;

public sealed class ModReleaseArtifactConfiguration : IEntityTypeConfiguration<ModReleaseArtifact>
{
    public void Configure(EntityTypeBuilder<ModReleaseArtifact> builder)
    {
        builder.HasIndex(a => a.ReleaseId).HasDatabaseName("ix_mod_release_artifacts_release_id");
        builder.HasIndex(a => a.DownloadUrl).IsUnique().HasDatabaseName("ix_mod_release_artifacts_download_url");
    }
}
