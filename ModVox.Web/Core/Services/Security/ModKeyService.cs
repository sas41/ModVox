using System.Security.Cryptography;
using System.Text;

namespace ModVox.Web.Security;

public sealed class ModKeyService : IModKeyService
{
    public string GeneratePlaintextKey()
    {
        var keyBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(keyBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public string Hash(string plaintextKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(plaintextKey));
        return Convert.ToHexString(bytes);
    }

    public bool Verify(string plaintextKey, string keyHash)
    {
        var computedHash = Hash(plaintextKey);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(keyHash));
    }
}
