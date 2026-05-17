using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.StaffControllerHandlers;

public sealed class ApproveModHandler
{
    private readonly IModRepository _modRepository;
    private readonly IAccountAuthorizationService _authorizationService;

    public ApproveModHandler(IModRepository modRepository, IAccountAuthorizationService authorizationService)
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

        if (currentUser.IsBanned(DateTimeOffset.UtcNow) || !_authorizationService.HasRole(currentUser, UserRoles.Admin, UserRoles.Moderator))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var mod = await _modRepository.GetByIdAsync(modId, cancellationToken);
        if (mod is null)
        {
            return Results.NotFound(new { message = "Mod not found." });
        }

        var updated = mod with { ModerationStatus = ModModerationStatus.Approved, UpdatedAt = DateTimeOffset.UtcNow };
        await _modRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new ApproveModResponse(updated.Id, updated.ModerationStatus));
    }
}

public sealed record ApproveModResponse(Guid ModId, string ModerationStatus);
