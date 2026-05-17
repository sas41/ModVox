using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.GamesControllerHandlers;

public sealed class GetAdminGameHandler
{
    private readonly IGameRepository _gameRepository;
    private readonly IAccountAuthorizationService _authorizationService;

    public GetAdminGameHandler(IGameRepository gameRepository, IAccountAuthorizationService authorizationService)
    {
        _gameRepository = gameRepository;
        _authorizationService = authorizationService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, Guid gameId, CancellationToken cancellationToken)
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

        var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (game is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new GetAdminGameResponse(game.Id, game.Name, game.Slug, game.IsHidden));
    }
}

public sealed record GetAdminGameResponse(Guid GameId, string Name, string Slug, bool IsHidden);
