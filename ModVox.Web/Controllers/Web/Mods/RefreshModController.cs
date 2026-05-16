using ModVox.Web.ApiModels;
using ModVox.Web.Refresh;
using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints;

public sealed class RefreshModEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/refresh/mod", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        RefreshRequestPayload _,
        IModRepository modRepository,
        IContentSyncService contentSyncService,
        IModKeyService modKeyService,
        CancellationToken cancellationToken)
    {
        var key = AuthHelpers.TryGetBearerToken(httpContext);
        if (string.IsNullOrWhiteSpace(key))
        {
            return Results.Unauthorized();
        }

        var hashedKey = modKeyService.Hash(key);
        var mod = await modRepository.GetByHashedKeyAsync(hashedKey, cancellationToken);
        if (mod is null)
        {
            return Results.Unauthorized();
        }

        var result = await contentSyncService.SyncAsync(mod, cancellationToken);
        if (!string.Equals(result.Status, "updated", StringComparison.OrdinalIgnoreCase))
        {
            return Results.UnprocessableEntity(new
            {
                message = result.Message ?? "Refresh failed.",
                step = result.Step,
                code = result.ErrorCode
            });
        }

        return Results.Ok(new RefreshModResponse(
            mod.Id,
            result.Status,
            DateTimeOffset.UtcNow,
            result.ReleasesUpserted,
            "Refresh completed successfully."));
    }
}
