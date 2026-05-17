using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.ThunderstoreModels;

namespace ModVox.Web.Endpoints.ThunderstoreControllerHandlers;

public sealed class GetExperimentalPackageHandler
{
    private readonly IModRepository _modRepository;
    private readonly IModReleaseRepository _releaseRepository;

    public GetExperimentalPackageHandler(IModRepository modRepository, IModReleaseRepository releaseRepository)
    {
        _modRepository = modRepository;
        _releaseRepository = releaseRepository;
    }

    public async Task<IResult> HandleAsync(string @namespace, string name, CancellationToken cancellationToken)
    {
        var mod = await FindVisibleModAsync(@namespace, name, cancellationToken);
        if (mod is null)
        {
            return Results.NotFound();
        }

        var releases = await _releaseRepository.ListByModIdAsync(mod.Id, cancellationToken);
        var latest = releases.Where(x => !x.IsHidden).OrderByDescending(x => x.PublishedAt).FirstOrDefault();
        if (latest is null)
        {
            return Results.NotFound();
        }

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

    private async Task<Mod?> FindVisibleModAsync(string @namespace, string name, CancellationToken cancellationToken)
    {
        var mods = await _modRepository.ListVisibleAsync(cancellationToken);
        return mods.FirstOrDefault(m =>
            string.Equals(m.Owner, @namespace, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(m.Repository, name, StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildRepoUrl(string owner, string repository) =>
        $"https://github.com/{owner}/{repository}";

    private static ThunderstorePackageVersionDto BuildPackageVersion(Mod mod, ModRelease release, string downloadUrl)
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

public sealed record GetExperimentalPackageResponse(ThunderstorePackageDetailDto Package);
