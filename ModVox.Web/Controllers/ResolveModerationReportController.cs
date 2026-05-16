using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class ResolveModerationReportEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/moderation/reports/{reportId:guid}/resolve", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid reportId,
        ResolveReportRequest request,
        IModReportRepository modReportRepository,
        IAccountAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var currentUser = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (currentUser is null)
        {
            return Results.Unauthorized();
        }

        if (currentUser.IsBanned(DateTimeOffset.UtcNow) || !authorizationService.HasRole(currentUser, UserRoles.Admin, UserRoles.Moderator))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var report = await modReportRepository.GetByIdAsync(reportId, cancellationToken);
        if (report is null)
        {
            return Results.NotFound(new { message = "Report not found." });
        }

        if (string.Equals(report.Status, ModReportStatus.Resolved, StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new { message = "Report already resolved." });
        }

        var resolvedAt = DateTimeOffset.UtcNow;
        var updated = report with
        {
            Status = ModReportStatus.Resolved,
            ResolvedByUserId = currentUser.Id,
            ResolvedAt = resolvedAt,
            ResolutionNote = request.ResolutionNote?.Trim()
        };

        await modReportRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new ResolveReportResponse(updated.Id, updated.Status, currentUser.Id, resolvedAt));
    }
}
