using Microsoft.AspNetCore.Mvc.RazorPages;
using ModVox.Web.Domain;
using ModVox.Web.Providers;
using ModVox.Web.Repositories;

namespace ModVox.Web.Pages;

public sealed class ModModel : PageModel
{
    private readonly IModRepository _modRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IRepositoryProviderRegistry _providerRegistry;
    private readonly IUserRepository _userRepository;
    private readonly IModReleaseRepository _modReleaseRepository;

    public ModModel(
        IModRepository modRepository,
        IGameRepository gameRepository,
        IRepositoryProviderRegistry providerRegistry,
        IUserRepository userRepository,
        IModReleaseRepository modReleaseRepository)
    {
        _modRepository = modRepository;
        _gameRepository = gameRepository;
        _providerRegistry = providerRegistry;
        _userRepository = userRepository;
        _modReleaseRepository = modReleaseRepository;
    }

    public bool IsMissing { get; private set; }
    public Guid ModId { get; private set; }
    public string GameName { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Owner { get; private set; } = string.Empty;
    public string Repository { get; private set; } = string.Empty;
    public string RepoUrl { get; private set; } = string.Empty;
    public string DiscussionsUrl { get; private set; } = string.Empty;
    public string IssuesUrl { get; private set; } = string.Empty;
    public string CreatorDisplayName { get; private set; } = string.Empty;
    public string CreatorProfileUrl { get; private set; } = string.Empty;
    public string Provider { get; private set; } = string.Empty;
    public string ReadmeHtml { get; private set; } = string.Empty;
    public string ChangelogHtml { get; private set; } = string.Empty;
    public List<string> ImageUrls { get; } = new();
    public List<CreditRow> Credits { get; } = new();
    public List<ReleaseRow> Releases { get; } = new();

    public async Task OnGetAsync(Guid gameId, Guid modId, CancellationToken cancellationToken)
    {
        var mod = await _modRepository.GetByGameAndIdAsync(gameId, modId, includeHidden: false, cancellationToken);
        if (mod is null)
        {
            IsMissing = true;
            return;
        }

        ModId = mod.Id;
        Name = mod.Name;
        Description = mod.Description;
        Owner = mod.Owner;
        Repository = mod.Repository;
        Provider = mod.Provider;
        ReadmeHtml = mod.ReadmeHtml ?? string.Empty;
        ChangelogHtml = mod.ChangelogHtml ?? string.Empty;
        RepoUrl = BuildRepositoryUrl(mod);
        DiscussionsUrl = BuildDiscussionsUrl(mod);
        IssuesUrl = BuildIssuesUrl(mod);

        var game = await _gameRepository.GetByIdAsync(mod.GameId, cancellationToken);
        GameName = game?.Name ?? "Unknown Game";

        var creator = await _userRepository.GetByIdAsync(mod.MaintainerUserId, cancellationToken);
        CreatorDisplayName = creator?.DisplayName ?? creator?.Username ?? mod.MaintainerUserId.ToString();
        CreatorProfileUrl = $"/user/{mod.MaintainerUserId}";

        var creditedUserIds = mod.Credits.Keys.ToArray();
        var creditedUsersById = await _userRepository.GetByIdsAsync(creditedUserIds, cancellationToken);

        foreach (var entry in mod.Credits)
        {
            creditedUsersById.TryGetValue(entry.Key, out var user);
            Credits.Add(new CreditRow(
                Name: user?.DisplayName ?? user?.Username ?? entry.Key.ToString(),
                Role: entry.Value,
                Url: user is null ? $"/user/{entry.Key}" : $"/user/{user.Id}"));
        }

        foreach (var entry in mod.ExternalCredits.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        {
            Credits.Add(new CreditRow(entry.Key, entry.Value, null));
        }

        var releases = await _modReleaseRepository.ListByModIdAsync(mod.Id, cancellationToken);
        foreach (var release in releases.Where(x => !x.IsHidden).OrderByDescending(x => x.PublishedAt))
        {
            Releases.Add(new ReleaseRow(
                release.TagName,
                string.IsNullOrWhiteSpace(release.Name) ? release.TagName : release.Name,
                release.PublishedAt,
                release.IsPrerelease,
                release.Artifacts
                    .OrderBy(a => a.FileName, StringComparer.OrdinalIgnoreCase)
                    .Select(a => new ReleaseArtifactRow(a.FileName, a.DownloadUrl, a.Size))
                    .ToList()));
        }

        try
        {
            var repoProvider = _providerRegistry.Get(mod.Provider);
            var coordinates = new RepositoryCoordinates(mod.Provider, mod.Owner, mod.Repository, mod.DefaultRef);
            var images = await repoProvider.ListFolderAsync(coordinates, mod.ImagesFolder, cancellationToken);

            ImageUrls.AddRange(images
                .Where(x => !x.IsDirectory)
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => ToImageSourceUrl(mod.Provider, x.PublicUrl)));
        }
        catch
        {
            // Best-effort image listing only.
        }
    }

    private static string BuildRepositoryUrl(ModRecord mod)
    {
        if (string.Equals(mod.Provider, "github", StringComparison.OrdinalIgnoreCase))
        {
            return $"https://github.com/{mod.Owner}/{mod.Repository}";
        }

        return string.Empty;
    }

    private static string BuildDiscussionsUrl(ModRecord mod)
    {
        var repoUrl = BuildRepositoryUrl(mod);
        return string.IsNullOrWhiteSpace(repoUrl) ? string.Empty : $"{repoUrl}/discussions";
    }

    private static string BuildIssuesUrl(ModRecord mod)
    {
        var repoUrl = BuildRepositoryUrl(mod);
        return string.IsNullOrWhiteSpace(repoUrl) ? string.Empty : $"{repoUrl}/issues";
    }

    private static string ToImageSourceUrl(string provider, Uri publicUrl)
    {
        if (!string.Equals(provider, "github", StringComparison.OrdinalIgnoreCase))
        {
            return publicUrl.ToString();
        }

        var uriText = publicUrl.ToString();
        if (!publicUrl.Host.Contains("github.com", StringComparison.OrdinalIgnoreCase))
        {
            return uriText;
        }

        var hasQuery = uriText.Contains('?', StringComparison.Ordinal);
        var hasRaw = uriText.Contains("raw=true", StringComparison.OrdinalIgnoreCase);
        if (hasRaw)
        {
            return uriText;
        }

        return hasQuery ? $"{uriText}&raw=true" : $"{uriText}?raw=true";
    }

    public sealed record CreditRow(string Name, string Role, string? Url);
    public sealed record ReleaseArtifactRow(string FileName, string DownloadUrl, long Size);
    public sealed record ReleaseRow(
        string TagName,
        string Name,
        DateTimeOffset PublishedAt,
        bool IsPrerelease,
        IReadOnlyList<ReleaseArtifactRow> Artifacts);
}
