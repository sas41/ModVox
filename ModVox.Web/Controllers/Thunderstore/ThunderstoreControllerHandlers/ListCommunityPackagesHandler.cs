using ModVox.Web.Repositories;
using ModVox.Web.ThunderstoreModels;

namespace ModVox.Web.Endpoints.ThunderstoreControllerHandlers;

public sealed class ListCommunityPackagesHandler
{
    private readonly IGameRepository _gameRepository;
    private readonly IModRepository _modRepository;
    private readonly IModReleaseRepository _releaseRepository;

    public ListCommunityPackagesHandler(
        IGameRepository gameRepository,
        IModRepository modRepository,
        IModReleaseRepository releaseRepository)
    {
        _gameRepository = gameRepository;
        _modRepository = modRepository;
        _releaseRepository = releaseRepository;
    }

    public async Task<IResult> HandleAsync(string communityIdentifier, CancellationToken cancellationToken)
    {
        var games = await _gameRepository.ListAsync(cancellationToken);
        var game = games.FirstOrDefault(g => string.Equals(g.Slug, communityIdentifier, StringComparison.OrdinalIgnoreCase));
        if (game is null)
        {
            return Results.Ok(Array.Empty<object>());
        }

        var mods = await _modRepository.ListVisibleByGameIdAsync(game.Id, cancellationToken);
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

    private static ThunderstorePackageListItemDto BuildPackageListItem(ModVox.Web.Domain.Mod mod, IReadOnlyList<ModVox.Web.Domain.ModRelease> visibleReleases)
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

public sealed record ListCommunityPackagesResponse(IReadOnlyList<ThunderstorePackageListItemDto> Items);
