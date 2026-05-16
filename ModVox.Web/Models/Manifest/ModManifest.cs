namespace ModVox.Web.Manifest;

/// <summary>
/// Parsed and validated representation of a modvox.json manifest file.
/// All required fields are guaranteed non-null/non-empty when this record is constructed.
/// </summary>
public sealed record ModManifest(
    string? Verify,
    string Name,
    string? Description,
    string DefaultRef,
    string Readme,
    string Changelog,
    string Images,
    IReadOnlyList<string> Tags,
    IReadOnlyDictionary<string, string> Credits);

/// <summary>
/// Discriminated result of reading and parsing a manifest file from a repository.
/// </summary>
public abstract record ManifestReadResult
{
    private ManifestReadResult() { }

    /// <summary>The manifest file was not found in the repository.</summary>
    public sealed record NotFound : ManifestReadResult;

    /// <summary>The manifest file was found but failed validation.</summary>
    public sealed record Invalid(string Reason) : ManifestReadResult;

    /// <summary>The manifest was successfully read, parsed and validated.</summary>
    public sealed record Valid(ModManifest Manifest, IReadOnlyList<Guid> ResolvedTagIds) : ManifestReadResult;
}
