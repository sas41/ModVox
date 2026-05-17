using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.ThunderstoreModels;

namespace ModVox.Web.Endpoints.ThunderstoreControllerHandlers;

public sealed class ListPackagesHandler
{
    private readonly IModRepository _modRepository;
    private readonly IModReleaseRepository _releaseRepository;

    public ListPackagesHandler(IModRepository modRepository, IModReleaseRepository releaseRepository)
    {
        _modRepository = modRepository;
        _releaseRepository = releaseRepository;
    }

    public async Task<IResult> HandleAsync(CancellationToken cancellationToken)
    {
        var mods = await _modRepository.ListVisibleAsync(cancellationToken);
        var releasesByMod = await _releaseRepository.ListByModIdsAsync(mods.Select(m => m.Id), cancellationToken);
        var rows = new List<ThunderstorePackageListItemDto>();

        foreach (var mod in mods)
        {
            if (!releasesByMod.TryGetValue(mod.Id, out var releases))
            {
                continue;
            }

            var visibleReleases = releases.Where(x => !x.IsHidden).OrderByDescending(x => x.PublishedAt).ToList();
            if (visibleReleases.Count == 0)
            {
                continue;
            }

            rows.Add(BuildPackageListItem(mod, visibleReleases));
        }

        return Results.Ok(rows);
    }

    private static ThunderstorePackageListItemDto BuildPackageListItem(Mod mod, IReadOnlyList<ModRelease> visibleReleases)
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
}

public sealed record ListPackagesResponse(IReadOnlyList<ThunderstorePackageListItemDto> Items);
