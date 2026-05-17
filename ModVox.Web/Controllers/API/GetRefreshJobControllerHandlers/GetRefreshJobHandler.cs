using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.GetRefreshJobControllerHandlers;

public sealed class GetRefreshJobHandler
{
    private readonly IRefreshJobRepository _refreshJobRepository;
    private readonly IModRepository _modRepository;
    private readonly IModKeyService _modKeyService;
    private readonly IAccountAuthorizationService _authorizationService;

    public GetRefreshJobHandler(
        IRefreshJobRepository refreshJobRepository,
        IModRepository modRepository,
        IModKeyService modKeyService,
        IAccountAuthorizationService authorizationService)
    {
        _refreshJobRepository = refreshJobRepository;
        _modRepository = modRepository;
        _modKeyService = modKeyService;
        _authorizationService = authorizationService;
    }

    public async Task<IResult> HandleAsync(Guid jobId, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        Mod? keyedMod = null;
        if (currentUser is null)
        {
            var key = AuthHelpers.TryGetBearerToken(httpContext);
            if (string.IsNullOrWhiteSpace(key))
            {
                return Results.Unauthorized();
            }

            keyedMod = await _modRepository.GetByHashedKeyAsync(_modKeyService.Hash(key), cancellationToken);
            if (keyedMod is null)
            {
                return Results.Unauthorized();
            }
        }

        var job = await _refreshJobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job is null)
        {
            return Results.NotFound(new { message = "Job not found." });
        }

        if (currentUser is not null)
        {
            var mod = await _modRepository.GetByIdAsync(job.ModId, cancellationToken);
            if (mod is null)
            {
                return Results.NotFound(new { message = "Job not found." });
            }

            var isOwner = mod.MaintainerUserId == currentUser.Id;
            var isStaff = _authorizationService.HasRole(currentUser, UserRoles.Admin, UserRoles.Moderator);
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

public sealed record RefreshJobResponse(
    Guid JobId,
    Guid ModId,
    string Status,
    string? Result,
    string? Error,
    DateTimeOffset EnqueuedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt);
