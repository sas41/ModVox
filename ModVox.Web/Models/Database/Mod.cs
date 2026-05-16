using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModVox.Web.Domain;

[Table("mods")]
public record class Mod
{
    [Key]
    [Column("id")]
    public Guid Id { get; init; }

    [Column("game_id")]
    public Guid GameId { get; init; }

    [Column("maintainer_user_id")]
    public Guid MaintainerUserId { get; init; }

    [Column("provider")]
    [MaxLength(64)]
    [Required]
    public string Provider { get; init; } = string.Empty;

    [Column("owner")]
    [MaxLength(256)]
    [Required]
    public string Owner { get; init; } = string.Empty;

    [Column("repository")]
    [MaxLength(256)]
    [Required]
    public string Repository { get; init; } = string.Empty;

    [Column("default_ref")]
    [MaxLength(256)]
    [Required]
    public string DefaultRef { get; init; } = string.Empty;

    [Column("name")]
    [MaxLength(256)]
    [Required]
    public string Name { get; init; } = string.Empty;

    [Column("description")]
    [Required]
    public string Description { get; init; } = string.Empty;

    [Column("readme_path")]
    [MaxLength(512)]
    [Required]
    public string ReadmePath { get; init; } = string.Empty;

    [Column("changelog_path")]
    [MaxLength(512)]
    [Required]
    public string ChangelogPath { get; init; } = string.Empty;

    [Column("images_folder")]
    [MaxLength(512)]
    [Required]
    public string ImagesFolder { get; init; } = string.Empty;

    [Column("readme_markdown")]
    public string? ReadmeMarkdown { get; init; }

    [Column("readme_html")]
    public string? ReadmeHtml { get; init; }

    [Column("changelog_markdown")]
    public string? ChangelogMarkdown { get; init; }

    [Column("changelog_html")]
    public string? ChangelogHtml { get; init; }

    [Column("content_fetched_at")]
    public DateTimeOffset? ContentFetchedAt { get; init; }

    [Column("tag_ids", TypeName = "uuid[]")]
    public IReadOnlyList<Guid> TagIds { get; init; } = Array.Empty<Guid>();

    [Column("credits", TypeName = "jsonb")]
    public IReadOnlyDictionary<Guid, string> Credits { get; init; } = new Dictionary<Guid, string>();

    [Column("external_credits", TypeName = "jsonb")]
    public IReadOnlyDictionary<string, string> ExternalCredits { get; init; } = new Dictionary<string, string>();

    [Column("download_count")]
    public long DownloadCount { get; init; }

    [Column("moderation_status")]
    [MaxLength(32)]
    [Required]
    public string ModerationStatus { get; init; } = ModModerationStatus.Unverified;

    [Column("verify_token")]
    [MaxLength(128)]
    [Required]
    public string VerifyToken { get; init; } = string.Empty;

    [Column("key_hash")]
    [MaxLength(128)]
    public string? KeyHash { get; init; }

    [Column("key_version")]
    public int KeyVersion { get; init; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; init; }

    [Column("last_accepted_refresh_at")]
    public DateTimeOffset? LastAcceptedRefreshAt { get; init; }

    // Navigation properties
    public virtual Game? Game { get; protected set; }
    public virtual UserAccount? MaintainerUser { get; protected set; }
    public virtual ICollection<ModRelease> Releases { get; protected set; } = new List<ModRelease>();
    public virtual ICollection<ModReport> Reports { get; protected set; } = new List<ModReport>();
    public virtual ICollection<RefreshJob> RefreshJobs { get; protected set; } = new List<RefreshJob>();

    protected Mod() { }

    public Mod(
        Guid Id, Guid GameId, Guid MaintainerUserId,
        string Provider, string Owner, string Repository, string DefaultRef,
        string Name, string Description,
        string ReadmePath, string ChangelogPath, string ImagesFolder,
        string? ReadmeMarkdown, string? ReadmeHtml,
        string? ChangelogMarkdown, string? ChangelogHtml,
        DateTimeOffset? ContentFetchedAt,
        IReadOnlyList<Guid> TagIds, IReadOnlyDictionary<Guid, string> Credits,
        IReadOnlyDictionary<string, string> ExternalCredits,
        long DownloadCount, string ModerationStatus,
        string VerifyToken, string? KeyHash,
        DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt,
        DateTimeOffset? LastAcceptedRefreshAt, int KeyVersion)
    {
        this.Id = Id; this.GameId = GameId; this.MaintainerUserId = MaintainerUserId;
        this.Provider = Provider; this.Owner = Owner; this.Repository = Repository; this.DefaultRef = DefaultRef;
        this.Name = Name; this.Description = Description;
        this.ReadmePath = ReadmePath; this.ChangelogPath = ChangelogPath; this.ImagesFolder = ImagesFolder;
        this.ReadmeMarkdown = ReadmeMarkdown; this.ReadmeHtml = ReadmeHtml;
        this.ChangelogMarkdown = ChangelogMarkdown; this.ChangelogHtml = ChangelogHtml;
        this.ContentFetchedAt = ContentFetchedAt;
        this.TagIds = TagIds; this.Credits = Credits; this.ExternalCredits = ExternalCredits;
        this.DownloadCount = DownloadCount; this.ModerationStatus = ModerationStatus;
        this.VerifyToken = VerifyToken; this.KeyHash = KeyHash;
        this.CreatedAt = CreatedAt; this.UpdatedAt = UpdatedAt;
        this.LastAcceptedRefreshAt = LastAcceptedRefreshAt; this.KeyVersion = KeyVersion;
    }
}
