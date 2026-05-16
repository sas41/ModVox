using Microsoft.Extensions.Options;
using ModVox.Web.ApiModels;
using ModVox.Web.Config;
using ModVox.Web.Domain;
using ModVox.Web.Refresh;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class RefreshModEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/refresh/mod", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        RefreshRequestPayload payload,
        IModRepository modRepository,
        IRefreshJobRepository refreshJobRepository,
        IRefreshQueue refreshQueue,
        IModKeyService modKeyService,
        IOptions<RefreshOptions> refreshOptions,
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

        var activeJob = await refreshJobRepository.FindActiveByModAndKeyAsync(mod.Id, payload.IdempotencyKey, cancellationToken);
        if (activeJob is not null)
        {
            return Results.Accepted($"/api/v1/refresh/jobs/{activeJob.Id}", new RefreshModResponse(activeJob.Id, activeJob.Status, activeJob.EnqueuedAt));
        }

        var now = DateTimeOffset.UtcNow;
        var minInterval = TimeSpan.FromMinutes(refreshOptions.Value.MinIntervalMinutes);
        if (mod.LastAcceptedRefreshAt.HasValue)
        {
            var elapsed = now - mod.LastAcceptedRefreshAt.Value;
            if (elapsed < minInterval)
            {
                var retry = minInterval - elapsed;
                return Results.Json(
                    new { message = "Refresh cooldown active.", retry_after_seconds = Math.Max(1, (int)Math.Ceiling(retry.TotalSeconds)) },
                    statusCode: StatusCodes.Status429TooManyRequests);
            }
        }

        var job = new RefreshJobRecord
        {
            Id = Guid.NewGuid(),
            ModId = mod.Id,
            Provider = mod.Provider,
            Owner = mod.Owner,
            Repository = mod.Repository,
            Ref = mod.DefaultRef,
            EnqueuedAt = now,
            IdempotencyKey = payload.IdempotencyKey
        };

        await refreshJobRepository.AddAsync(job, cancellationToken);
        await refreshQueue.QueueAsync(job, cancellationToken);

        var response = new RefreshModResponse(job.Id, job.Status, job.EnqueuedAt);
        return Results.Accepted($"/api/v1/refresh/jobs/{job.Id}", response);
    }
}
