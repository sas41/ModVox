namespace ModVox.Web.Endpoints;

public sealed class ModsController : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        new RegisterModEndpoint().MapEndpoint(app);
        new RefreshManifestEndpoint().MapEndpoint(app);
        new RotateModKeyEndpoint().MapEndpoint(app);
        new RotateVerifyTokenEndpoint().MapEndpoint(app);
        new RefreshModEndpoint().MapEndpoint(app);
        new GetRefreshJobEndpoint().MapEndpoint(app);
        new RevokeModKeyEndpoint().MapEndpoint(app);
        new ListGameModsEndpoint().MapEndpoint(app);
        new GetModByGameEndpoint().MapEndpoint(app);
        new IncrementModDownloadEndpoint().MapEndpoint(app);
        new ApproveModEndpoint().MapEndpoint(app);
        new HideModEndpoint().MapEndpoint(app);
        new UnhideModEndpoint().MapEndpoint(app);
        new DeleteModEndpoint().MapEndpoint(app);
        new CreateModReportEndpoint().MapEndpoint(app);
        new ListMaintainerModsEndpoint().MapEndpoint(app);
        new ListModReleasesEndpoint().MapEndpoint(app);
        new HideReleaseEndpoint().MapEndpoint(app);
        new UnhideReleaseEndpoint().MapEndpoint(app);
        new DeleteReleaseEndpoint().MapEndpoint(app);
    }
}
