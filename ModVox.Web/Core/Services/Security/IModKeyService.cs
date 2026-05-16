namespace ModVox.Web.Security;

public interface IModKeyService
{
    string GeneratePlaintextKey();
    string Hash(string plaintextKey);
    bool Verify(string plaintextKey, string keyHash);
}
