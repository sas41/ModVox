namespace ModVox.Web.Endpoints;

public sealed class StaffController : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        new BanUserEndpoint().MapEndpoint(app);
        new UnbanUserEndpoint().MapEndpoint(app);
        new GetModerationReportsEndpoint().MapEndpoint(app);
        new ResolveModerationReportEndpoint().MapEndpoint(app);
        new CreateTagEndpoint().MapEndpoint(app);
        new ListTagsEndpoint().MapEndpoint(app);
        new UpdateTagEndpoint().MapEndpoint(app);
        new DeleteTagEndpoint().MapEndpoint(app);
        new ExportAuditLogEndpoint().MapEndpoint(app);
        new PurgeAuditLogEndpoint().MapEndpoint(app);
    }
}
