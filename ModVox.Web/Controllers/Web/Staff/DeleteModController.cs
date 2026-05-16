using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class DeleteModEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/admin/mods/{modId:guid}", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid modId,
        IModRepository modRepository,
        IAccountAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var currentUser = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (currentUser is null)
        {
            return Results.Unauthorized();
        }

        if (currentUser.IsBanned(DateTimeOffset.UtcNow) || !authorizationService.HasRole(currentUser, UserRoles.Admin))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var mod = await modRepository.GetByIdAsync(modId, cancellationToken);
        if (mod is null)
        {
            return Results.NotFound(new { message = "Mod not found." });
        }

        if (!string.Equals(mod.ModerationStatus, ModModerationStatus.Hidden, StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new { message = "Only hidden mods can be permanently deleted." });
        }

        await modRepository.DeleteAsync(modId, cancellationToken);
        return Results.NoContent();
    }
}
