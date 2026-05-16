using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModVox.Web.Domain;

[Table("tags")]
public record class Tag
{
    [Key]
    [Column("id")]
    public Guid Id { get; init; }

    [Column("label")]
    [MaxLength(128)]
    [Required]
    public string Label { get; init; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; init; }

    protected Tag() { }

    public Tag(Guid id, string label, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        Id = id; Label = label; CreatedAt = createdAt; UpdatedAt = updatedAt;
    }
}
