using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModVox.Web.Domain;

[Table("games")]
public record class Game
{
    [Key]
    [Column("id")]
    public Guid Id { get; init; }

    [Column("slug")]
    [MaxLength(128)]
    [Required]
    public string Slug { get; init; } = string.Empty;

    [Column("name")]
    [MaxLength(256)]
    [Required]
    public string Name { get; init; } = string.Empty;

    [Column("is_hidden")]
    public bool IsHidden { get; init; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; init; }

    // Navigation properties
    public virtual ICollection<Mod> Mods { get; protected set; } = new List<Mod>();

    protected Game() { }

    public Game(Guid Id, string Slug, string Name, bool IsHidden, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
    {
        this.Id = Id; this.Slug = Slug; this.Name = Name; this.IsHidden = IsHidden;
        this.CreatedAt = CreatedAt; this.UpdatedAt = UpdatedAt;
    }
}
