namespace ModVox.Web.ApiModels;

public sealed class RegisterModRequest
{
    public Guid GameId { get; init; }
    public string RepositoryUrl { get; init; } = string.Empty;

    /// <summary>
    /// Optional ref/branch used only for the initial manifest fetch.
    /// If null or empty, "HEAD" is used (resolves to the repo's default branch).
    /// After the manifest is read, default_ref is taken from the manifest itself.
    /// </summary>
    public string? InitialRef { get; init; }
}
