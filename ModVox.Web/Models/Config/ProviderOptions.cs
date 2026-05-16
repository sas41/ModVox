namespace ModVox.Web.Config;

public sealed class ProviderOptions
{
    public const string SectionName = "Providers";

    public GitHubOptions GitHub { get; set; } = new();
}

public sealed class GitHubOptions
{
    public string ApiBaseUrl { get; set; } = "https://api.github.com";
    public string RawBaseUrl { get; set; } = "https://raw.githubusercontent.com";
    public int TimeoutSeconds { get; set; } = 15;
}
