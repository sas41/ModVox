using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.ThunderstoreModels;

namespace ModVox.Web.Endpoints.ThunderstoreControllerHandlers;

public sealed class GetExperimentalPackageVersionHandler
{
    private readonly IModRepository _modRepository;
    private readonly IModReleaseRepository _releaseRepository;

    public GetExperimentalPackageVersionHandler(IModRepository modRepository, IModReleaseRepository releaseRepository)
    {
        _modRepository = modRepository;
        _releaseRepository = releaseRepository;
    }

    public async Task<IResult> HandleAsync(string @namespace, string name, string version, CancellationToken cancellationToken)
    {
        var mod = await FindVisibleModAsync(@namespace, name, cancellationToken);
        if (mod is null)
        {
            return Results.NotFound();
        }

        var releases = await _releaseRepository.ListByModIdAsync(mod.Id, cancellationToken);
        var release = releases
            .Where(x => !x.IsHidden)
            .FirstOrDefault(x => string.Equals(x.TagName, version, StringComparison.OrdinalIgnoreCase));
        if (release is null)
        {
            return Results.NotFound();
        }

        var firstArtifact = release.Artifacts.FirstOrDefault();
        var payload = BuildPackageVersion(mod, release, firstArtifact?.DownloadUrl ?? string.Empty);
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

public sealed record GetExperimentalPackageVersionResponse(ThunderstorePackageVersionDto Version);
