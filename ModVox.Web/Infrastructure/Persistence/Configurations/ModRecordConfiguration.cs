using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModVox.Web.Domain;

namespace ModVox.Web.Infrastructure.Persistence.Configurations;

public sealed class ModConfiguration : IEntityTypeConfiguration<Mod>
{
    public void Configure(EntityTypeBuilder<Mod> builder)
    {
        // TagIds stored as a Postgres UUID array
        builder.Property(m => m.TagIds)
            .HasConversion(
                v => v.ToArray(),
                v => v.ToList().AsReadOnly())
            .Metadata.SetValueComparer(new ValueComparer<IReadOnlyList<Guid>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                v => v.Aggregate(0, (h, g) => HashCode.Combine(h, g.GetHashCode())),
                v => v.ToList().AsReadOnly()));

        // Credits stored as JSONB: { "guid": "text", ... }
        builder.Property(m => m.Credits)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(
                    v.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value),
                    (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer
                    .Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null)!
                    .ToDictionary(kv => Guid.Parse(kv.Key), kv => kv.Value)
                    as IReadOnlyDictionary<Guid, string>)
            .Metadata.SetValueComparer(new ValueComparer<IReadOnlyDictionary<Guid, string>>(
                (a, b) => a != null && b != null && a.Count == b.Count && !a.Except(b).Any(),
                v => v.Aggregate(0, (h, kv) => HashCode.Combine(h, kv.Key.GetHashCode(), kv.Value.GetHashCode())),
                v => new Dictionary<Guid, string>(v)));

        builder.Property(m => m.ExternalCredits)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(
                    v.ToDictionary(kv => kv.Key, kv => kv.Value),
                    (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer
                    .Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null)!
                    as IReadOnlyDictionary<string, string>)
            .Metadata.SetValueComparer(new ValueComparer<IReadOnlyDictionary<string, string>>(
                (a, b) => a != null && b != null && a.Count == b.Count && !a.Except(b).Any(),
                v => v.Aggregate(0, (h, kv) => HashCode.Combine(h, kv.Key.GetHashCode(), kv.Value.GetHashCode())),
                v => new Dictionary<string, string>(v)));

        builder.HasIndex(m => new { m.Provider, m.Owner, m.Repository })
            .IsUnique()
            .HasDatabaseName("ix_mods_coordinates");
        builder.HasIndex(m => m.GameId).HasDatabaseName("ix_mods_game_id");
        builder.HasIndex(m => m.MaintainerUserId).HasDatabaseName("ix_mods_maintainer_user_id");
        builder.HasIndex(m => m.ModerationStatus).HasDatabaseName("ix_mods_moderation_status");
        builder.HasIndex(m => m.KeyHash).HasDatabaseName("ix_mods_key_hash");

        builder.HasMany(m => m.Releases)
            .WithOne(r => r.Mod)
            .HasForeignKey(r => r.ModId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Reports)
            .WithOne(r => r.Mod)
            .HasForeignKey(r => r.ModId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.RefreshJobs)
            .WithOne(j => j.Mod)
            .HasForeignKey(j => j.ModId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
