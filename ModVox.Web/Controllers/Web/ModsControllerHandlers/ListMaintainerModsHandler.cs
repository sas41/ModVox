using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.ModsControllerHandlers;

public sealed class ListMaintainerModsHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IUserRepository _userRepository;
    private readonly IModRepository _modRepository;

    public ListMaintainerModsHandler(
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
        var items = mods.Select(x => new ListMaintainerModsResponse(x.Id, x.GameId, x.Owner + "/" + x.Repository, !string.IsNullOrWhiteSpace(x.KeyHash)))
            .ToList();

        return Results.Ok(items);
    }
}

public sealed record ListMaintainerModsResponse(Guid ModId, Guid GameId, string Name, bool KeyActive);
