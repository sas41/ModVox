using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.ThunderstoreModels;

namespace ModVox.Web.Endpoints.ThunderstoreControllerHandlers;

public sealed class GetExperimentalChangelogHandler
{
    private readonly IModRepository _modRepository;
    private readonly IModReleaseRepository _releaseRepository;

    public GetExperimentalChangelogHandler(IModRepository modRepository, IModReleaseRepository releaseRepository)
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
        var exists = releases.Any(x => !x.IsHidden && string.Equals(x.TagName, version, StringComparison.OrdinalIgnoreCase));
        if (!exists)
        {
            return Results.NotFound();
        }

        return Results.Ok(new ThunderstoreMarkdownDto(mod.ChangelogMarkdown ?? string.Empty));
    }

    private async Task<Mod?> FindVisibleModAsync(string @namespace, string name, CancellationToken cancellationToken)
    {
        var mods = await _modRepository.ListVisibleAsync(cancellationToken);
        return mods.FirstOrDefault(m =>
            string.Equals(m.Owner, @namespace, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(m.Repository, name, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed record GetExperimentalChangelogResponse(ThunderstoreMarkdownDto Changelog);
