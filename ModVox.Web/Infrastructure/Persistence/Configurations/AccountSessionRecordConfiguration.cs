using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModVox.Web.Domain;

namespace ModVox.Web.Infrastructure.Persistence.Configurations;

public sealed class AccountSessionRecordConfiguration : IEntityTypeConfiguration<AccountSessionRecord>
{
    public void Configure(EntityTypeBuilder<AccountSessionRecord> builder)
    {
        builder.HasIndex(s => s.UserId).HasDatabaseName("ix_account_sessions_user_id");
        builder.HasIndex(s => s.ExpiresAt).HasDatabaseName("ix_account_sessions_expires_at");
    }
}
