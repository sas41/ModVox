using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class RevokeAllUserModKeysHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IUserRepository _userRepository;
    private readonly IModRepository _modRepository;

    public RevokeAllUserModKeysHandler(
        IAccountAuthorizationService authorizationService,
        IUserRepository userRepository,
        IModRepository modRepository)
    {
        _authorizationService = authorizationService;
        _userRepository = userRepository;
        _modRepository = modRepository;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, Guid userId, CancellationToken cancellationToken)
    {
        var actor = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (actor is null)
        {
            return Results.Unauthorized();
        }

        if (!_authorizationService.HasRole(actor, UserRoles.Admin))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var maintainer = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (maintainer is null)
        {
            return Results.NotFound();
        }

        var mods = await _modRepository.ListByMaintainerUserIdAsync(userId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var count = 0;

        foreach (var mod in mods)
        {
            if (string.IsNullOrWhiteSpace(mod.KeyHash))
            {
                continue;
            }

            var updated = mod with { KeyHash = null, UpdatedAt = now };
            await _modRepository.UpdateAsync(updated, cancellationToken);
            count++;
        }

        return Results.Ok(new RevokeAllUserModKeysResponse(count));
    }
}

public sealed record RevokeAllUserModKeysResponse(int RevokedCount);
