using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class RotateModKeyEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/mods/{modId:guid}/keys/rotate", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid modId,
        IModRepository modRepository,
        IAccountAuthorizationService authorizationService,
        IModKeyService modKeyService,
        CancellationToken cancellationToken)
    {
        var user = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        if (user.IsBanned(DateTimeOffset.UtcNow))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var mod = await modRepository.GetByIdAsync(modId, cancellationToken);
        if (mod is null)
        {
            return Results.NotFound(new { message = "Mod not found." });
        }

        var isAdmin = authorizationService.HasRole(user, UserRoles.Admin);
        var isMaintainerOwner = authorizationService.HasRole(user, UserRoles.Maintainer) && mod.MaintainerUserId == user.Id;
        if (!isAdmin && !isMaintainerOwner)
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        if (!isAdmin)
        {
            var hasBlockedMods = await modRepository.HasFlaggedOrHiddenModsForMaintainerAsync(user.Id, cancellationToken);
            if (hasBlockedMods)
            {
                return Results.Json(new { message = "Maintainer has flagged/hidden mods and cannot add or update mods." }, statusCode: StatusCodes.Status403Forbidden);
            }
        }

        var newKey = modKeyService.GeneratePlaintextKey();
        var newHash = modKeyService.Hash(newKey);

        var updated = mod with
        {
            KeyHash = newHash,
            KeyVersion = mod.KeyVersion + 1,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await modRepository.UpdateAsync(updated, cancellationToken);

        var response = new RotateModKeyResponse(updated.Id, newKey, updated.KeyVersion);
        return Results.Ok(response);
    }
}
