using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModVox.Web.Domain;

[Table("users")]
public record class UserAccount
{
    [Key]
    [Column("id")]
    public Guid Id { get; init; }

    [Column("username")]
    [MaxLength(64)]
    [Required]
    public string Username { get; init; } = string.Empty;

    [Column("display_name")]
    [MaxLength(128)]
    [Required]
    public string DisplayName { get; init; } = string.Empty;

    [Column("email")]
    [MaxLength(256)]
    [Required]
    public string Email { get; init; } = string.Empty;

    [Column("password_hash")]
    [Required]
    public string PasswordHash { get; init; } = string.Empty;

    [Column("role")]
    [MaxLength(32)]
    [Required]
    public string Role { get; init; } = string.Empty;

    [Column("must_change_credentials")]
    public bool MustChangeCredentials { get; init; }

    [Column("ban_type")]
    [MaxLength(32)]
    [Required]
    public string BanType { get; init; } = UserBanTypes.None;

    [Column("ban_expires_at")]
    public DateTimeOffset? BanExpiresAt { get; init; }

    [Column("session_version")]
    public int SessionVersion { get; init; }

    [Column("is_deleted")]
    public bool IsDeleted { get; init; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; init; }

    // Navigation properties
    public virtual ICollection<ModRecord> Mods { get; protected set; } = new List<ModRecord>();
    public virtual ICollection<AccountSessionRecord> Sessions { get; protected set; } = new List<AccountSessionRecord>();
    public virtual ICollection<ModReportRecord> Reports { get; protected set; } = new List<ModReportRecord>();

    protected UserAccount() { }

    public UserAccount(
        Guid Id, string Username, string DisplayName, string Email,
        string PasswordHash, string Role, bool MustChangeCredentials,
        string BanType, DateTimeOffset? BanExpiresAt, int SessionVersion,
        bool IsDeleted, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt)
    {
        this.Id = Id; this.Username = Username; this.DisplayName = DisplayName; this.Email = Email;
        this.PasswordHash = PasswordHash; this.Role = Role; this.MustChangeCredentials = MustChangeCredentials;
        this.BanType = BanType; this.BanExpiresAt = BanExpiresAt; this.SessionVersion = SessionVersion;
        this.IsDeleted = IsDeleted; this.CreatedAt = CreatedAt; this.UpdatedAt = UpdatedAt;
    }

    public bool IsAdmin => string.Equals(Role, UserRoles.Admin, StringComparison.OrdinalIgnoreCase);

    public bool IsBanned(DateTimeOffset now)
    {
        if (string.Equals(BanType, UserBanTypes.Permanent, StringComparison.OrdinalIgnoreCase))
            return true;
        if (!string.Equals(BanType, UserBanTypes.Temporary, StringComparison.OrdinalIgnoreCase))
            return false;
        return BanExpiresAt.HasValue && BanExpiresAt.Value > now;
    }
}

public static class UserRoles
{
    public const string Admin = "admin";
    public const string Moderator = "moderator";
    public const string Maintainer = "maintainer";
    public const string User = "user";
}

public static class UserBanTypes
{
    public const string None = "none";
    public const string Temporary = "temporary";
    public const string Permanent = "permanent";
}
