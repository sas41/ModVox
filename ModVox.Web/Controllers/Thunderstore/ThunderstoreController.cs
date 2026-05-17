using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ModVox.Web.Endpoints.ThunderstoreControllerHandlers;

namespace ModVox.Web.Endpoints;

[ApiController]
public sealed class ThunderstoreController : ControllerBase
{
    private THandler CreateHandler<THandler>() where THandler : notnull =>
        ActivatorUtilities.CreateInstance<THandler>(HttpContext.RequestServices);

    [HttpGet("/api/v1/package/")]
    public Task<IResult> ListPackagesAsync(CancellationToken cancellationToken) =>
        CreateHandler<ListPackagesHandler>().HandleAsync(cancellationToken);

    [HttpGet("/c/{community_identifier}/api/v1/package/")]
    public Task<IResult> ListCommunityPackagesAsync(
        string community_identifier,
        CancellationToken cancellationToken) =>
        CreateHandler<ListCommunityPackagesHandler>().HandleAsync(community_identifier, cancellationToken);

    [HttpGet("/api/experimental/package-index/")]
    public Task WritePackageIndexAsync(CancellationToken cancellationToken) =>
        CreateHandler<WritePackageIndexHandler>().HandleAsync(HttpContext, cancellationToken);

    [HttpGet("/api/experimental/package/{namespace}/{name}/")]
    public Task<IResult> GetExperimentalPackageAsync(
        string @namespace,
        string name,
        CancellationToken cancellationToken) =>
        CreateHandler<GetExperimentalPackageHandler>().HandleAsync(@namespace, name, cancellationToken);

    [HttpGet("/api/experimental/package/{namespace}/{name}/{version}/")]
    public Task<IResult> GetExperimentalPackageVersionAsync(
        string @namespace,
        string name,
        string version,
        CancellationToken cancellationToken) =>
        CreateHandler<GetExperimentalPackageVersionHandler>().HandleAsync(@namespace, name, version, cancellationToken);

    [HttpGet("/api/experimental/package/{namespace}/{name}/{version}/readme/")]
    public Task<IResult> GetExperimentalReadmeAsync(
        string @namespace,
        string name,
        string version,
        CancellationToken cancellationToken) =>
        CreateHandler<GetExperimentalReadmeHandler>().HandleAsync(@namespace, name, version, cancellationToken);

    [HttpGet("/api/experimental/package/{namespace}/{name}/{version}/changelog/")]
    public Task<IResult> GetExperimentalChangelogAsync(
        string @namespace,
        string name,
        string version,
        CancellationToken cancellationToken) =>
        CreateHandler<GetExperimentalChangelogHandler>().HandleAsync(@namespace, name, version, cancellationToken);
}
