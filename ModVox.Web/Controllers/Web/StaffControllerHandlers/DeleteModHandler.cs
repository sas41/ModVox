using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.StaffControllerHandlers;

public sealed class DeleteModHandler
{
    private readonly IModRepository _modRepository;
    private readonly IAccountAuthorizationService _authorizationService;

    public DeleteModHandler(IModRepository modRepository, IAccountAuthorizationService authorizationService)
    {
        _modRepository = modRepository;
        _authorizationService = authorizationService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, Guid modId, CancellationToken cancellationToken)
    {
        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (currentUser is null)
        {
            return Results.Unauthorized();
        }

        if (currentUser.IsBanned(DateTimeOffset.UtcNow) || !_authorizationService.HasRole(currentUser, UserRoles.Admin))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var mod = await _modRepository.GetByIdAsync(modId, cancellationToken);
        if (mod is null)
        {
            return Results.NotFound(new { message = "Mod not found." });
        }

        if (!string.Equals(mod.ModerationStatus, ModModerationStatus.Hidden, StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new { message = "Only hidden mods can be permanently deleted." });
        }

        await _modRepository.DeleteAsync(modId, cancellationToken);
        return Results.NoContent();
    }
}

public sealed record DeleteModResponse(bool NoContent = true);
