using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ModVox.Web.Endpoints.StaffControllerHandlers;

namespace ModVox.Web.Endpoints;

[ApiController]
public sealed class StaffController : ControllerBase
{
    private THandler CreateHandler<THandler>() where THandler : notnull =>
        ActivatorUtilities.CreateInstance<THandler>(HttpContext.RequestServices);

    [HttpPost("/api/v1/mods/{modId:guid}/moderation/approve")]
    public Task<IResult> ApproveModAsync(
        Guid modId,
        CancellationToken cancellationToken) =>
        CreateHandler<ApproveModHandler>().HandleAsync(HttpContext, modId, cancellationToken);

    [HttpPost("/api/v1/mods/{modId:guid}/moderation/hide")]
    public Task<IResult> HideModAsync(
        Guid modId,
        CancellationToken cancellationToken) =>
        CreateHandler<HideModHandler>().HandleAsync(HttpContext, modId, cancellationToken);

    [HttpPost("/api/v1/mods/{modId:guid}/moderation/unhide")]
    public Task<IResult> UnhideModAsync(
        Guid modId,
        CancellationToken cancellationToken) =>
        CreateHandler<UnhideModHandler>().HandleAsync(HttpContext, modId, cancellationToken);

    [HttpDelete("/api/v1/admin/mods/{modId:guid}")]
    public Task<IResult> DeleteModAsync(
        Guid modId,
        CancellationToken cancellationToken) =>
        CreateHandler<DeleteModHandler>().HandleAsync(HttpContext, modId, cancellationToken);

    [HttpGet("/api/v1/moderation/reports")]
    public Task<IResult> GetModerationReportsAsync(
        CancellationToken cancellationToken) =>
        CreateHandler<GetModerationReportsHandler>().HandleAsync(HttpContext, cancellationToken);

    [HttpPost("/api/v1/moderation/reports/{reportId:guid}/resolve")]
    public Task<IResult> ResolveModerationReportAsync(
        Guid reportId,
        ResolveModerationReportHandler.ResolveModerationReportRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<ResolveModerationReportHandler>().HandleAsync(HttpContext, reportId, request, cancellationToken);

    [HttpPost("/api/v1/admin/users/{userId:guid}/ban")]
    public Task<IResult> BanUserAsync(
        Guid userId,
        BanUserHandler.BanUserRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<BanUserHandler>().HandleAsync(HttpContext, userId, request, cancellationToken);

    [HttpPost("/api/v1/admin/users/{userId:guid}/unban")]
    public Task<IResult> UnbanUserAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        CreateHandler<UnbanUserHandler>().HandleAsync(HttpContext, userId, cancellationToken);

    [HttpGet("/api/v1/tags")]
    public Task<IResult> ListTagsAsync(
        CancellationToken cancellationToken) =>
        CreateHandler<ListTagsHandler>().HandleAsync(cancellationToken);

    [HttpPost("/api/v1/admin/tags")]
    public Task<IResult> CreateTagAsync(
        CreateTagHandler.CreateTagRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<CreateTagHandler>().HandleAsync(HttpContext, request, cancellationToken);

    [HttpPost("/api/v1/admin/tags/{tagId:guid}")]
    public Task<IResult> UpdateTagAsync(
        Guid tagId,
        UpdateTagHandler.UpdateTagRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<UpdateTagHandler>().HandleAsync(HttpContext, tagId, request, cancellationToken);

    [HttpDelete("/api/v1/admin/tags/{tagId:guid}")]
    public Task<IResult> DeleteTagAsync(
        Guid tagId,
        CancellationToken cancellationToken) =>
        CreateHandler<DeleteTagHandler>().HandleAsync(HttpContext, tagId, cancellationToken);

    [HttpGet("/api/v1/admin/audit/export")]
    public Task<IResult> ExportAuditLogAsync(
        CancellationToken cancellationToken) =>
        CreateHandler<ExportAuditLogHandler>().HandleAsync(HttpContext, cancellationToken);

    [HttpPost("/api/v1/admin/audit/purge")]
    public Task<IResult> PurgeAuditLogAsync(
        CancellationToken cancellationToken) =>
        CreateHandler<PurgeAuditLogHandler>().HandleAsync(HttpContext, cancellationToken);
}
