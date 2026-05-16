using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModVox.Web.Domain;

namespace ModVox.Web.Infrastructure.Persistence.Configurations;

public sealed class RefreshJobRecordConfiguration : IEntityTypeConfiguration<RefreshJobRecord>
{
    public void Configure(EntityTypeBuilder<RefreshJobRecord> builder)
    {
        builder.HasIndex(j => j.ModId).HasDatabaseName("ix_refresh_jobs_mod_id");
        builder.HasIndex(j => j.Status).HasDatabaseName("ix_refresh_jobs_status");
        builder.HasIndex(j => new { j.ModId, j.IdempotencyKey }).HasDatabaseName("ix_refresh_jobs_mod_idempotency");
    }
}
