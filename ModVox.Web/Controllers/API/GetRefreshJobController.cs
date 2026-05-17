using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ModVox.Web.Endpoints.GetRefreshJobControllerHandlers;

namespace ModVox.Web.Endpoints;

[ApiController]
public sealed class GetRefreshJobController : ControllerBase
{
    private THandler CreateHandler<THandler>() where THandler : notnull =>
        ActivatorUtilities.CreateInstance<THandler>(HttpContext.RequestServices);

    [HttpGet("/api/v1/refresh/jobs/{jobId:guid}")]
    public Task<IResult> HandleAsync(Guid jobId, CancellationToken cancellationToken) =>
        CreateHandler<GetRefreshJobHandler>().HandleAsync(jobId, HttpContext, cancellationToken);
}
