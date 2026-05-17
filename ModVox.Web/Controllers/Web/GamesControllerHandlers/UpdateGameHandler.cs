using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.GamesControllerHandlers;

public sealed class UpdateGameHandler
{
    private readonly IGameRepository _gameRepository;
    private readonly IAccountAuthorizationService _authorizationService;

    public UpdateGameHandler(IGameRepository gameRepository, IAccountAuthorizationService authorizationService)
    {
        _gameRepository = gameRepository;
        _authorizationService = authorizationService;
    }

    public async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid gameId,
        UpdateGameRequest request,
        CancellationToken cancellationToken)
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

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Slug))
        {
            return Results.BadRequest(new { message = "name and slug are required." });
        }

        var slug = request.Slug.Trim().ToLowerInvariant();
        var existing = await _gameRepository.GetBySlugAsync(slug, cancellationToken);
        if (existing is not null && existing.Id != game.Id)
        {
            return Results.Conflict(new { message = "Game slug already exists." });
        }

        var updated = game with
        {
            Name = request.Name.Trim(),
            Slug = slug,
            IsHidden = request.IsHidden,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _gameRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new UpdateGameResponse(updated.Id, updated.Name, updated.Slug, updated.IsHidden));
    }

    public sealed class UpdateGameRequest
    {
        public string Name { get; init; } = string.Empty;
        public string Slug { get; init; } = string.Empty;
        public bool IsHidden { get; init; }
    }
}

public sealed record UpdateGameResponse(Guid GameId, string Name, string Slug, bool IsHidden);
