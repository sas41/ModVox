using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.GamesControllerHandlers;

public sealed class CreateGameHandler
{
    private readonly IGameRepository _gameRepository;
    private readonly IAccountAuthorizationService _authorizationService;

    public CreateGameHandler(IGameRepository gameRepository, IAccountAuthorizationService authorizationService)
    {
        _gameRepository = gameRepository;
        _authorizationService = authorizationService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, CreateGameRequest request, CancellationToken cancellationToken)
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

        if (string.IsNullOrWhiteSpace(request.Slug) || string.IsNullOrWhiteSpace(request.Name))
        {
            return Results.BadRequest(new { message = "slug and name are required." });
        }

        var slug = request.Slug.Trim().ToLowerInvariant();
        var existing = await _gameRepository.GetBySlugAsync(slug, cancellationToken);
        if (existing is not null)
        {
            return Results.Conflict(new { message = "Game slug already exists." });
        }

        var now = DateTimeOffset.UtcNow;
        var game = new Game(Guid.NewGuid(), slug, request.Name.Trim(), IsHidden: false, now, now);
        await _gameRepository.AddAsync(game, cancellationToken);

        return Results.Created($"/api/v1/games/{game.Id}", new CreateGameResponse(game.Id, game.Slug, game.Name));
    }

    public sealed class CreateGameRequest
    {
        public string Slug { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
    }
}

public sealed record CreateGameResponse(Guid GameId, string Slug, string Name);
