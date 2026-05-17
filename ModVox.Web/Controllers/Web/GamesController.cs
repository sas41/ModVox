using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ModVox.Web.Endpoints.GamesControllerHandlers;

namespace ModVox.Web.Endpoints;

[ApiController]
public sealed class GamesController : ControllerBase
{
    private THandler CreateHandler<THandler>() where THandler : notnull =>
        ActivatorUtilities.CreateInstance<THandler>(HttpContext.RequestServices);

    [HttpPost("/api/v1/admin/games")]
    public Task<IResult> CreateGameAsync(
        CreateGameHandler.CreateGameRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<CreateGameHandler>().HandleAsync(HttpContext, request, cancellationToken);

    [HttpGet("/api/v1/games")]
    public Task<IResult> ListGamesAsync(
        string? q,
        CancellationToken cancellationToken) =>
        CreateHandler<ListGamesHandler>().HandleAsync(q, cancellationToken);

    [HttpGet("/api/v1/admin/games")]
    public Task<IResult> ListAdminGamesAsync(
        string? q,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken) =>
        CreateHandler<ListAdminGamesHandler>().HandleAsync(HttpContext, q, page, pageSize, cancellationToken);

    [HttpGet("/api/v1/admin/games/{gameId:guid}")]
    public Task<IResult> GetAdminGameAsync(
        Guid gameId,
        CancellationToken cancellationToken) =>
        CreateHandler<GetAdminGameHandler>().HandleAsync(HttpContext, gameId, cancellationToken);

    [HttpPost("/api/v1/admin/games/{gameId:guid}")]
    public Task<IResult> UpdateGameAsync(
        Guid gameId,
        UpdateGameHandler.UpdateGameRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<UpdateGameHandler>().HandleAsync(HttpContext, gameId, request, cancellationToken);
}
