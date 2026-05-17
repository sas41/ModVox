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
        var count = await modRepository.IncrementDownloadCountAsync(gameId, modId, cancellationToken);
        if (!count.HasValue)
        {
            return Results.NotFound();
        }

        return Results.Ok(new { mod_id = modId, download_count = count.Value });
    }
}
