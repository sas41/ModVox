using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModVox.Web.Domain;

[Table("mod_releases")]
public record class ModRelease
{
    [Key]
    [Column("id")]
    public Guid Id { get; init; }

    [Column("mod_id")]
    public Guid ModId { get; init; }

    [Column("tag_name")]
    [MaxLength(256)]
    [Required]
    public string TagName { get; init; } = string.Empty;

    [Column("name")]
    [MaxLength(256)]
    [Required]
    public string Name { get; init; } = string.Empty;

    [Column("is_prerelease")]
    public bool IsPrerelease { get; init; }

    [Column("published_at")]
    public DateTimeOffset PublishedAt { get; init; }

    [Column("fetched_at")]
    public DateTimeOffset FetchedAt { get; init; }

    [Column("is_hidden")]
    public bool IsHidden { get; init; }

    // Navigation properties
    public virtual Mod? Mod { get; protected set; }
    public virtual ICollection<ModReleaseArtifact> Artifacts { get; protected set; } = new List<ModReleaseArtifact>();

    protected ModRelease() { }

    public ModRelease(
        Guid id, Guid modId, string tagName, string name,
        bool isPrerelease, DateTimeOffset publishedAt, DateTimeOffset fetchedAt)
    {
        Id = id; ModId = modId; TagName = tagName; Name = name;
        IsPrerelease = isPrerelease; PublishedAt = publishedAt; FetchedAt = fetchedAt;
    }
}
