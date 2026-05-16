namespace ModVox.Web.Providers;

public sealed record ProviderFileListItem(
    string Path,
    string Name,
    bool IsDirectory,
    Uri PublicUrl);
