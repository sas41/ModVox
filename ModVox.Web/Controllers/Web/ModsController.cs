using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ModVox.Web.Endpoints.ModsControllerHandlers;
using ModVox.Web.Refresh;

namespace ModVox.Web.Endpoints;

[ApiController]
public sealed class ModsController : ControllerBase
{
    private THandler CreateHandler<THandler>() where THandler : notnull =>
        ActivatorUtilities.CreateInstance<THandler>(HttpContext.RequestServices);

    [HttpPost("/api/v1/mods")]
    public Task<IResult> RegisterModAsync(
        RegisterModHandler.RegisterModRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<RegisterModHandler>().HandleAsync(HttpContext, request, cancellationToken);

    [HttpPost("/api/v1/mods/{modId:guid}/refresh")]
    [HttpPost("/api/v1/mods/{modId:guid}/manifest/refresh")]
    public Task<IResult> RefreshManifestAsync(
        Guid modId,
        CancellationToken cancellationToken) =>
        CreateHandler<RefreshManifestHandler>().HandleAsync(modId, HttpContext, cancellationToken);

    [HttpPost("/api/v1/mods/{modId:guid}/keys/rotate")]
    public Task<IResult> RotateModKeyAsync(
        Guid modId,
        CancellationToken cancellationToken) =>
        CreateHandler<RotateModKeyHandler>().HandleAsync(HttpContext, modId, cancellationToken);

    [HttpPost("/api/v1/mods/{modId:guid}/verify-token/rotate")]
    public Task<IResult> RotateVerifyTokenAsync(
        Guid modId,
        CancellationToken cancellationToken) =>
        CreateHandler<RotateVerifyTokenHandler>().HandleAsync(HttpContext, modId, cancellationToken);

    [HttpPost("/api/v1/refresh/mod")]
    public Task<IResult> RefreshModAsync(
        RefreshRequestPayload request,
        CancellationToken cancellationToken) =>
        CreateHandler<RefreshModHandler>().HandleAsync(HttpContext, request, cancellationToken);

    [HttpPost("/api/v1/mods/{modId:guid}/keys/revoke")]
    public Task<IResult> RevokeModKeyAsync(
        Guid modId,
        CancellationToken cancellationToken) =>
        CreateHandler<RevokeModKeyHandler>().HandleAsync(HttpContext, modId, cancellationToken);

    [HttpGet("/api/v1/games/{gameId:guid}/mods")]
    public Task<IResult> ListGameModsAsync(
        Guid gameId,
        string? q,
        CancellationToken cancellationToken) =>
        CreateHandler<ListGameModsHandler>().HandleAsync(HttpContext, gameId, q, cancellationToken);

    [HttpGet("/api/v1/games/{gameId:guid}/mods/{modId:guid}")]
    public Task<IResult> GetModByGameAsync(
        Guid gameId,
        Guid modId,
        CancellationToken cancellationToken) =>
        CreateHandler<GetModByGameHandler>().HandleAsync(HttpContext, gameId, modId, cancellationToken);

    [HttpPost("/api/v1/games/{gameId:guid}/mods/{modId:guid}/download")]
    public Task<IResult> IncrementModDownloadAsync(
        Guid gameId,
        Guid modId,
        CancellationToken cancellationToken) =>
        CreateHandler<IncrementModDownloadHandler>().HandleAsync(gameId, modId, cancellationToken);

    [HttpGet("/api/v1/admin/users/{userId:guid}/mods")]
    public Task<IResult> ListMaintainerModsAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        CreateHandler<ListMaintainerModsHandler>().HandleAsync(HttpContext, userId, cancellationToken);

    [HttpGet("/api/v1/mods/{modId:guid}/releases")]
    public Task<IResult> ListModReleasesAsync(
        Guid modId,
        CancellationToken cancellationToken) =>
        CreateHandler<ListModReleasesHandler>().HandleAsync(modId, HttpContext, cancellationToken);

    [HttpPost("/api/v1/releases/{releaseId:guid}/hide")]
    public Task<IResult> HideReleaseAsync(
        Guid releaseId,
        CancellationToken cancellationToken) =>
        CreateHandler<HideReleaseHandler>().HandleAsync(releaseId, HttpContext, cancellationToken);

    [HttpPost("/api/v1/releases/{releaseId:guid}/unhide")]
    public Task<IResult> UnhideReleaseAsync(
        Guid releaseId,
        CancellationToken cancellationToken) =>
        CreateHandler<UnhideReleaseHandler>().HandleAsync(releaseId, HttpContext, cancellationToken);

    [HttpDelete("/api/v1/releases/{releaseId:guid}")]
    public Task<IResult> DeleteReleaseAsync(
        Guid releaseId,
        CancellationToken cancellationToken) =>
        CreateHandler<DeleteReleaseHandler>().HandleAsync(releaseId, HttpContext, cancellationToken);

    [HttpPost("/api/v1/mods/{modId:guid}/reports")]
    public Task<IResult> CreateModReportAsync(
        Guid modId,
        CreateModReportHandler.CreateModReportRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<CreateModReportHandler>().HandleAsync(HttpContext, modId, request, cancellationToken);
}
