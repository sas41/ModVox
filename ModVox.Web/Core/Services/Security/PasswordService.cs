using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using ModVox.Web.Domain;

namespace ModVox.Web.Security;

public sealed class PasswordService : IPasswordService
{
    private static readonly UserAccount PlaceholderUser = new(
        Guid.Empty,
        "placeholder",
        "placeholder",
        "placeholder@local.modvox",
        string.Empty,
        UserRoles.User,
        MustChangeCredentials: false,
        BanType: UserBanTypes.None,
        BanExpiresAt: null,
        SessionVersion: 1,
        IsDeleted: false,
        DateTimeOffset.UnixEpoch,
        DateTimeOffset.UnixEpoch);

    private readonly IPasswordHasher<UserAccount> _passwordHasher;

    public PasswordService(IPasswordHasher<UserAccount> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public string Hash(string password)
    {
        return _passwordHasher.HashPassword(PlaceholderUser, password);
    }

    public bool Verify(string password, string hash)
    {
        var identityResult = _passwordHasher.VerifyHashedPassword(PlaceholderUser, hash, password);
        if (identityResult is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded)
        {
            return true;
        }

        return VerifyLegacyHash(password, hash);
    }

    private static bool VerifyLegacyHash(string password, string hash)
    {
        var parts = hash.Split(':');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        byte[] salt;
        byte[] expected;

        try
        {
            salt = Convert.FromBase64String(parts[1]);
            expected = Convert.FromBase64String(parts[2]);
        }
        catch
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
