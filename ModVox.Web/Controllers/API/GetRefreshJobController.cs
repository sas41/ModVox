using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class GetRefreshJobEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/refresh/jobs/{jobId:guid}", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        Guid jobId,
        HttpContext httpContext,
        IRefreshJobRepository refreshJobRepository,
        IModRepository modRepository,
        IModKeyService modKeyService,
        IAccountAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var currentUser = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        ModRecord? keyedMod = null;
        if (currentUser is null)
        {
            var key = AuthHelpers.TryGetBearerToken(httpContext);
            if (string.IsNullOrWhiteSpace(key))
            {
                return Results.Unauthorized();
            }

            keyedMod = await modRepository.GetByHashedKeyAsync(modKeyService.Hash(key), cancellationToken);
            if (keyedMod is null)
            {
                return Results.Unauthorized();
            }
        }

        var job = await refreshJobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job is null)
        {
            return Results.NotFound(new { message = "Job not found." });
        }

        if (currentUser is not null)
        {
            var mod = await modRepository.GetByIdAsync(job.ModId, cancellationToken);
            if (mod is null)
            {
                return Results.NotFound(new { message = "Job not found." });
            }

            var isOwner = mod.MaintainerUserId == currentUser.Id;
            var isStaff = authorizationService.HasRole(currentUser, UserRoles.Admin, UserRoles.Moderator);
            if (!isOwner && !isStaff)
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }
        }
        else if (keyedMod!.Id != job.ModId)
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var response = new RefreshJobResponse(
            job.Id,
            job.ModId,
            job.Status,
            job.Result,
            job.Error,
            job.EnqueuedAt,
            job.StartedAt,
            job.CompletedAt);

        return Results.Ok(response);
    }
}
