namespace ModVox.Web.Endpoints;

public sealed class ApiController : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        new GetManifestScaffoldEndpoint().MapEndpoint(app);
    }
}
