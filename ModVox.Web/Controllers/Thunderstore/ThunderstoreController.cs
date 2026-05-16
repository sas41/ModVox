using System.Text;
using System.Text.Json;
using ModVox.Web.Repositories;
using ModVox.Web.ThunderstoreModels;

namespace ModVox.Web.Endpoints;

public sealed class ThunderstoreController : IEndpoint
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/package/", ListPackagesAsync);
        app.MapGet("/c/{community_identifier}/api/v1/package/", ListCommunityPackagesAsync);

        app.MapGet("/api/experimental/package-index/", WritePackageIndexAsync);
        app.MapGet("/api/experimental/package/{namespace}/{name}/", GetExperimentalPackageAsync);
        app.MapGet("/api/experimental/package/{namespace}/{name}/{version}/", GetExperimentalPackageVersionAsync);
        app.MapGet("/api/experimental/package/{namespace}/{name}/{version}/readme/", GetExperimentalReadmeAsync);
        app.MapGet("/api/experimental/package/{namespace}/{name}/{version}/changelog/", GetExperimentalChangelogAsync);
    }

    private static async Task<IResult> ListPackagesAsync(
        IGameRepository gameRepository,
        IModRepository modRepository,
        IModReleaseRepository releaseRepository,
        CancellationToken cancellationToken)
    {
        var mods = await ListVisibleModsAsync(gameRepository, modRepository, cancellationToken);
        var rows = new List<ThunderstorePackageListItemDto>();

        foreach (var mod in mods)
        {
            var releases = await releaseRepository.ListByModIdAsync(mod.Id, cancellationToken);
            var visibleReleases = releases.Where(x => !x.IsHidden).OrderByDescending(x => x.PublishedAt).ToList();
            if (visibleReleases.Count == 0)
                continue;

            rows.Add(BuildPackageListItem(mod, visibleReleases));
        }

        return Results.Ok(rows);
    }

    private static async Task<IResult> ListCommunityPackagesAsync(
        string community_identifier,
        IGameRepository gameRepository,
        IModRepository modRepository,
        IModReleaseRepository releaseRepository,
        CancellationToken cancellationToken)
    {
        var games = await gameRepository.ListAsync(cancellationToken);
        var game = games.FirstOrDefault(g => string.Equals(g.Slug, community_identifier, StringComparison.OrdinalIgnoreCase));
        if (game is null)
            return Results.Ok(Array.Empty<object>());

        var mods = await modRepository.ListVisibleByGameIdAsync(game.Id, cancellationToken);
        var rows = new List<ThunderstorePackageListItemDto>();

        foreach (var mod in mods)
        {
            var releases = await releaseRepository.ListByModIdAsync(mod.Id, cancellationToken);
            var visibleReleases = releases.Where(x => !x.IsHidden).OrderByDescending(x => x.PublishedAt).ToList();
            if (visibleReleases.Count == 0)
                continue;

            rows.Add(BuildPackageListItem(mod, visibleReleases));
        }

        return Results.Ok(rows);
    }

    private static async Task WritePackageIndexAsync(
        HttpContext httpContext,
        IGameRepository gameRepository,
        IModRepository modRepository,
        IModReleaseRepository releaseRepository,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        httpContext.Response.ContentType = "application/x-ndjson";

        var mods = await ListVisibleModsAsync(gameRepository, modRepository, cancellationToken);
        foreach (var mod in mods)
        {
            var releases = await releaseRepository.ListByModIdAsync(mod.Id, cancellationToken);
            foreach (var release in releases.Where(x => !x.IsHidden))
            {
                var firstArtifact = release.Artifacts.FirstOrDefault();
                var entry = new ThunderstoreIndexEntryDto(
                    mod.Owner,
                    mod.Repository,
                    release.TagName,
                    "zip",
                    firstArtifact?.Size ?? 0,
                    Array.Empty<string>());

                var line = JsonSerializer.Serialize(entry, JsonOptions) + "\n";
                await httpContext.Response.WriteAsync(line, Encoding.UTF8, cancellationToken);
            }
        }
    }

    private static async Task<IResult> GetExperimentalPackageAsync(
        string @namespace,
        string name,
        IGameRepository gameRepository,
        IModRepository modRepository,
        IModReleaseRepository releaseRepository,
        CancellationToken cancellationToken)
    {
        var mod = await FindVisibleModAsync(@namespace, name, gameRepository, modRepository, cancellationToken);
        if (mod is null)
            return Results.NotFound();

        var releases = await releaseRepository.ListByModIdAsync(mod.Id, cancellationToken);
        var latest = releases.Where(x => !x.IsHidden).OrderByDescending(x => x.PublishedAt).FirstOrDefault();
        if (latest is null)
            return Results.NotFound();

        var latestArtifact = latest.Artifacts.FirstOrDefault();
        var payload = new ThunderstorePackageDetailDto(
            mod.Owner,
            mod.Repository,
            mod.Owner + "-" + mod.Repository,
            mod.Owner,
            $"/api/experimental/package/{mod.Owner}/{mod.Repository}/",
            mod.CreatedAt,
            mod.UpdatedAt,
            "0",
            false,
            false,
            mod.DownloadCount.ToString(),
            BuildPackageVersion(mod, latest, latestArtifact?.DownloadUrl ?? string.Empty),
            Array.Empty<object>());

        return Results.Ok(payload);
    }

    private static async Task<IResult> GetExperimentalPackageVersionAsync(
        string @namespace,
        string name,
        string version,
        IGameRepository gameRepository,
        IModRepository modRepository,
        IModReleaseRepository releaseRepository,
        CancellationToken cancellationToken)
    {
        var mod = await FindVisibleModAsync(@namespace, name, gameRepository, modRepository, cancellationToken);
        if (mod is null)
            return Results.NotFound();

        var releases = await releaseRepository.ListByModIdAsync(mod.Id, cancellationToken);
        var release = releases
            .Where(x => !x.IsHidden)
            .FirstOrDefault(x => string.Equals(x.TagName, version, StringComparison.OrdinalIgnoreCase));
        if (release is null)
            return Results.NotFound();

        var firstArtifact = release.Artifacts.FirstOrDefault();
        var payload = BuildPackageVersion(mod, release, firstArtifact?.DownloadUrl ?? string.Empty);

        return Results.Ok(payload);
    }

    private static async Task<IResult> GetExperimentalReadmeAsync(
        string @namespace,
        string name,
        string version,
        IGameRepository gameRepository,
        IModRepository modRepository,
        IModReleaseRepository releaseRepository,
        CancellationToken cancellationToken)
    {
        var mod = await FindVisibleModAsync(@namespace, name, gameRepository, modRepository, cancellationToken);
        if (mod is null)
            return Results.NotFound();

        var releases = await releaseRepository.ListByModIdAsync(mod.Id, cancellationToken);
        var exists = releases.Any(x => !x.IsHidden && string.Equals(x.TagName, version, StringComparison.OrdinalIgnoreCase));
        if (!exists)
            return Results.NotFound();

        return Results.Ok(new ThunderstoreMarkdownDto(mod.ReadmeMarkdown ?? string.Empty));
    }

    private static async Task<IResult> GetExperimentalChangelogAsync(
        string @namespace,
        string name,
        string version,
        IGameRepository gameRepository,
        IModRepository modRepository,
        IModReleaseRepository releaseRepository,
        CancellationToken cancellationToken)
    {
        var mod = await FindVisibleModAsync(@namespace, name, gameRepository, modRepository, cancellationToken);
        if (mod is null)
            return Results.NotFound();

        var releases = await releaseRepository.ListByModIdAsync(mod.Id, cancellationToken);
        var exists = releases.Any(x => !x.IsHidden && string.Equals(x.TagName, version, StringComparison.OrdinalIgnoreCase));
        if (!exists)
            return Results.NotFound();

        return Results.Ok(new ThunderstoreMarkdownDto(mod.ChangelogMarkdown ?? string.Empty));
    }

    private static async Task<IReadOnlyList<ModRecord>> ListVisibleModsAsync(
        IGameRepository gameRepository,
        IModRepository modRepository,
        CancellationToken cancellationToken)
    {
        var games = await gameRepository.ListAsync(cancellationToken);
        var mods = new List<ModRecord>();
        foreach (var game in games)
        {
            var byGame = await modRepository.ListVisibleByGameIdAsync(game.Id, cancellationToken);
            mods.AddRange(byGame);
        }

        return mods.GroupBy(m => m.Id).Select(x => x.First()).ToArray();
    }

    private static async Task<ModRecord?> FindVisibleModAsync(
        string @namespace,
        string name,
        IGameRepository gameRepository,
        IModRepository modRepository,
        CancellationToken cancellationToken)
    {
        var mods = await ListVisibleModsAsync(gameRepository, modRepository, cancellationToken);
        return mods.FirstOrDefault(m =>
            string.Equals(m.Owner, @namespace, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(m.Repository, name, StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildRepoUrl(string owner, string repository)
        => $"https://github.com/{owner}/{repository}";

    private static ThunderstorePackageListItemDto BuildPackageListItem(
        ModRecord mod,
        IReadOnlyList<ModReleaseRecord> visibleReleases)
    {
        var versions = visibleReleases.Select(r =>
                new ThunderstorePackageVersionListingDto(
                    r.PublishedAt,
                    0,
                    r.Artifacts.FirstOrDefault()?.DownloadUrl ?? string.Empty,
                    r.Artifacts.FirstOrDefault()?.DownloadUrl ?? string.Empty,
                    r.TagName))
            .ToArray();

        return new ThunderstorePackageListItemDto(
            mod.Repository,
            mod.Owner + "-" + mod.Repository,
            mod.Owner,
            $"/api/experimental/package/{mod.Owner}/{mod.Repository}/",
            string.Empty,
            mod.CreatedAt,
            mod.UpdatedAt,
            mod.Id,
            "0",
            false,
            false,
            false,
            Array.Empty<string>(),
            versions);
    }

    private static ThunderstorePackageVersionDto BuildPackageVersion(
        ModRecord mod,
        ModReleaseRecord release,
        string downloadUrl)
    {
        return new ThunderstorePackageVersionDto(
            mod.Owner,
            mod.Repository,
            release.TagName,
            mod.Owner + "-" + mod.Repository + "-" + release.TagName,
            string.IsNullOrWhiteSpace(mod.Description) ? mod.Repository : mod.Description,
            string.Empty,
            Array.Empty<string>(),
            downloadUrl,
            0,
            release.PublishedAt,
            BuildRepoUrl(mod.Owner, mod.Repository),
            true);
    }
}
