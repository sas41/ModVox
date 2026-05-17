using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModVox.Web.Domain;

namespace ModVox.Web.Infrastructure.Persistence.Configurations;

public sealed class ModReportConfiguration : IEntityTypeConfiguration<ModReport>
{
    public void Configure(EntityTypeBuilder<ModReport> builder)
    {
        builder.HasIndex(r => r.ModId).HasDatabaseName("ix_mod_reports_mod_id");
        builder.HasIndex(r => r.Status).HasDatabaseName("ix_mod_reports_status");

        // ResolvedByUser is an optional second FK to users — not navigated from UserAccount
        builder.HasOne(r => r.ResolvedByUser)
            .WithMany()
            .HasForeignKey(r => r.ResolvedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
