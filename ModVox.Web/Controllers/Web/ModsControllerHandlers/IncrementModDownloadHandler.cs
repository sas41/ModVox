using ModVox.Web.Repositories;

namespace ModVox.Web.Endpoints.ModsControllerHandlers;

public sealed class IncrementModDownloadHandler
{
    private readonly IModRepository _modRepository;

    public IncrementModDownloadHandler(IModRepository modRepository)
    {
        _modRepository = modRepository;
    }

    public async Task<IResult> HandleAsync(Guid gameId, Guid modId, CancellationToken cancellationToken)
    {
        var count = await _modRepository.IncrementDownloadCountAsync(gameId, modId, cancellationToken);
        if (!count.HasValue)
        {
            return Results.NotFound();
        }

        return Results.Ok(new IncrementModDownloadResponse(modId, count.Value));
    }
}

public sealed record IncrementModDownloadResponse(Guid ModId, long DownloadCount);
