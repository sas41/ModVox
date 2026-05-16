using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using ModVox.Web.Config;

namespace ModVox.Web.Providers;

public sealed class GitHubRepositoryProvider : IRepositoryProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GitHubOptions _options;

    public GitHubRepositoryProvider(IHttpClientFactory httpClientFactory, IOptions<ProviderOptions> providerOptions)
    {
        _httpClientFactory = httpClientFactory;
        _options = providerOptions.Value.GitHub;
    }

    public string ProviderName => "github";

    public async Task<string?> GetFileContentAsync(RepositoryCoordinates coordinates, string path, CancellationToken cancellationToken)
    {
        var client = CreateClient();
        var rawUrl = $"{_options.RawBaseUrl.TrimEnd('/')}/{coordinates.Owner}/{coordinates.Repository}/{coordinates.RefName}/{path.TrimStart('/')}";

        using var response = await client.GetAsync(rawUrl, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProviderFileListItem>> ListFolderAsync(RepositoryCoordinates coordinates, string path, CancellationToken cancellationToken)
    {
        var client = CreateClient();
        var url = $"{_options.ApiBaseUrl.TrimEnd('/')}/repos/{coordinates.Owner}/{coordinates.Repository}/contents/{path.TrimStart('/')}?ref={Uri.EscapeDataString(coordinates.RefName)}";

        using var response = await client.GetAsync(url, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Array.Empty<ProviderFileListItem>();
        }

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var items = await JsonSerializer.DeserializeAsync<List<GitHubContentItem>>(stream, JsonOptions, cancellationToken) ?? new();

        return items.Select(x => new ProviderFileListItem(
            x.Path,
            x.Name,
            string.Equals(x.Type, "dir", StringComparison.OrdinalIgnoreCase),
            BuildContentPublicUrl(coordinates, x.Path, x.HtmlUrl))).ToArray();
    }

    public async Task<IReadOnlyList<RepositoryRelease>> ListReleasesAsync(RepositoryCoordinates coordinates, CancellationToken cancellationToken)
    {
        var client = CreateClient();
        var url = $"{_options.ApiBaseUrl.TrimEnd('/')}/repos/{coordinates.Owner}/{coordinates.Repository}/releases";

        using var response = await client.GetAsync(url, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Array.Empty<RepositoryRelease>();
        }

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var items = await JsonSerializer.DeserializeAsync<List<GitHubRelease>>(stream, JsonOptions, cancellationToken) ?? new();

        return items
            .Where(r => !r.Draft)
            .Select(release => new RepositoryRelease(
                release.TagName,
                release.Name,
                release.Prerelease,
                release.PublishedAt,
                release.Assets.Select(asset => new ReleaseArtifact(
                    asset.Name,
                    asset.ContentType,
                    asset.Size,
                    new Uri(asset.BrowserDownloadUrl))).ToArray())).ToArray();
    }

    public Uri ResolvePublicFileUrl(RepositoryCoordinates coordinates, string path)
    {
        return new Uri($"https://github.com/{coordinates.Owner}/{coordinates.Repository}/blob/{coordinates.RefName}/{path.TrimStart('/')}");
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient(nameof(GitHubRepositoryProvider));
        client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        client.DefaultRequestHeaders.UserAgent.Clear();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ModVox", "0.1"));
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return client;
    }

    private static Uri BuildContentPublicUrl(RepositoryCoordinates coordinates, string path, string? htmlUrl)
    {
        if (!string.IsNullOrWhiteSpace(htmlUrl) && Uri.TryCreate(htmlUrl, UriKind.Absolute, out var parsed))
        {
            return parsed;
        }

        return new Uri($"https://github.com/{coordinates.Owner}/{coordinates.Repository}/blob/{coordinates.RefName}/{path.TrimStart('/')}");
    }

    private sealed class GitHubContentItem
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("path")]
        public string Path { get; init; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; init; } = string.Empty;
    }

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; init; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; init; }

        [JsonPropertyName("draft")]
        public bool Draft { get; init; }

        [JsonPropertyName("published_at")]
        public DateTimeOffset PublishedAt { get; init; }

        [JsonPropertyName("assets")]
        public List<GitHubReleaseAsset> Assets { get; init; } = new();
    }

    private sealed class GitHubReleaseAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("content_type")]
        public string ContentType { get; init; } = "application/octet-stream";

        [JsonPropertyName("size")]
        public long Size { get; init; }

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; init; } = string.Empty;
    }
}
