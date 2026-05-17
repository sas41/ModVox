using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints.ModsControllerHandlers;

public sealed class RotateModKeyHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IModRepository _modRepository;
    private readonly IModKeyService _modKeyService;

    public RotateModKeyHandler(
        IAccountAuthorizationService authorizationService,
        IModRepository modRepository,
        IModKeyService modKeyService)
    {
        _authorizationService = authorizationService;
        _modRepository = modRepository;
        _modKeyService = modKeyService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, Guid modId, CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        if (user.IsBanned(DateTimeOffset.UtcNow))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var mod = await _modRepository.GetByIdAsync(modId, cancellationToken);
        if (mod is null)
        {
            return Results.NotFound(new { message = "Mod not found." });
        }

        var isAdmin = _authorizationService.HasRole(user, UserRoles.Admin);
        var isMaintainerOwner = _authorizationService.HasRole(user, UserRoles.Maintainer) && mod.MaintainerUserId == user.Id;
        if (!isAdmin && !isMaintainerOwner)
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        if (!isAdmin)
        {
            var hasBlockedMods = await _modRepository.HasFlaggedOrHiddenModsForMaintainerAsync(user.Id, cancellationToken);
            if (hasBlockedMods)
            {
                return Results.Json(new { message = "Maintainer has flagged/hidden mods and cannot add or update mods." }, statusCode: StatusCodes.Status403Forbidden);
            }
        }

        var newKey = _modKeyService.GeneratePlaintextKey();
        var newHash = _modKeyService.Hash(newKey);

        var updated = mod with
        {
            KeyHash = newHash,
            KeyVersion = mod.KeyVersion + 1,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _modRepository.UpdateAsync(updated, cancellationToken);

        var response = new RotateModKeyResponse(updated.Id, newKey, updated.KeyVersion);
        return Results.Ok(response);
    }
}

public sealed record RotateModKeyResponse(Guid ModId, string Key, int KeyVersion);
