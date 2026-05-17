using System.Text;
using System.Text.Json;
using ModVox.Web.Repositories;
using ModVox.Web.ThunderstoreModels;

namespace ModVox.Web.Endpoints.ThunderstoreControllerHandlers;

public sealed class WritePackageIndexHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IModRepository _modRepository;
    private readonly IModReleaseRepository _releaseRepository;

    public WritePackageIndexHandler(IModRepository modRepository, IModReleaseRepository releaseRepository)
    {
        _modRepository = modRepository;
        _releaseRepository = releaseRepository;
    }

    public async Task HandleAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        httpContext.Response.ContentType = "application/x-ndjson";

        var mods = await _modRepository.ListVisibleAsync(cancellationToken);
        var releasesByMod = await _releaseRepository.ListByModIdsAsync(mods.Select(m => m.Id), cancellationToken);
        foreach (var mod in mods)
        {
            if (!releasesByMod.TryGetValue(mod.Id, out var releases))
            {
                continue;
            }

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
}

public sealed record WritePackageIndexResponse(bool Streamed = true);
