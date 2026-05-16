namespace ModVox.Web.Domain;

public sealed record UserAccount(
    Guid Id,
    string Username,
    string DisplayName,
    string Email,
    string PasswordHash,
    string Role,
    bool MustChangeCredentials,
    string BanType,
    DateTimeOffset? BanExpiresAt,
    int SessionVersion,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public bool IsAdmin => string.Equals(Role, UserRoles.Admin, StringComparison.OrdinalIgnoreCase);

    public bool IsBanned(DateTimeOffset now)
    {
        if (string.Equals(BanType, UserBanTypes.Permanent, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.Equals(BanType, UserBanTypes.Temporary, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

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
