using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ModVox.Web.Endpoints.GetManifestScaffoldControllerHandlers;

namespace ModVox.Web.Endpoints;

[ApiController]
public sealed class GetManifestScaffoldController : ControllerBase
{
    private THandler CreateHandler<THandler>() where THandler : notnull =>
        ActivatorUtilities.CreateInstance<THandler>(HttpContext.RequestServices);

    [HttpGet("/api/v1/manifest/scaffold")]
    public IResult HandleAsync() =>
        CreateHandler<GetManifestScaffoldHandler>().Handle();
}
