namespace ModVox.Web.Providers;

public sealed record RepositoryCoordinates(
    string Provider,
    string Owner,
    string Repository,
    string RefName);
