using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModVox.Web.Domain;

[Table("mod_release_artifacts")]
public record class ModReleaseArtifact
{
    [Key]
    [Column("id")]
    public Guid Id { get; init; }

    [Column("release_id")]
    public Guid ReleaseId { get; init; }

    [Column("file_name")]
    [MaxLength(512)]
    [Required]
    public string FileName { get; init; } = string.Empty;

    [Column("content_type")]
    [MaxLength(128)]
    [Required]
    public string ContentType { get; init; } = string.Empty;

    [Column("size")]
    public long Size { get; init; }

    [Column("download_url")]
    [MaxLength(2048)]
    [Required]
    public string DownloadUrl { get; init; } = string.Empty;

    // Navigation properties
    public virtual ModRelease? Release { get; protected set; }

    protected ModReleaseArtifact() { }

    public ModReleaseArtifact(
        Guid id, Guid releaseId, string fileName,
        string contentType, long size, string downloadUrl)
    {
        Id = id; ReleaseId = releaseId; FileName = fileName;
        ContentType = contentType; Size = size; DownloadUrl = downloadUrl;
    }
}
