using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.ModsControllerHandlers;

public sealed class RevokeModKeyHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IModRepository _modRepository;

    public RevokeModKeyHandler(IAccountAuthorizationService authorizationService, IModRepository modRepository)
    {
        _authorizationService = authorizationService;
        _modRepository = modRepository;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, Guid modId, CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var mod = await _modRepository.GetByIdAsync(modId, cancellationToken);
        if (mod is null)
        {
            return Results.NotFound();
        }

        var canManage = _authorizationService.HasRole(user, UserRoles.Admin) || mod.MaintainerUserId == user.Id;
        if (!canManage)
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var updated = mod with { KeyHash = null, UpdatedAt = DateTimeOffset.UtcNow };
        await _modRepository.UpdateAsync(updated, cancellationToken);
        return Results.NoContent();
    }
}

public sealed record RevokeModKeyResponse(bool NoContent = true);
