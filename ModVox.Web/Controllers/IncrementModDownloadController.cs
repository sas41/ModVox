using ModVox.Web.Repositories;

namespace ModVox.Web.Endpoints;

public sealed class IncrementModDownloadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/games/{gameId:guid}/mods/{modId:guid}/download", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(Guid gameId, Guid modId, IModRepository modRepository, CancellationToken cancellationToken)
    {
        var mod = await modRepository.GetByGameAndIdAsync(gameId, modId, includeHidden: true, cancellationToken);
        if (mod is null)
        {
            return Results.NotFound();
        }

        var updated = mod with { DownloadCount = mod.DownloadCount + 1, UpdatedAt = DateTimeOffset.UtcNow };
        await modRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new { mod_id = updated.Id, download_count = updated.DownloadCount });
    }
}
