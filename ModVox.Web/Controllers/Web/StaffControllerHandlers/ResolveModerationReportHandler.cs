using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.StaffControllerHandlers;

public sealed class ResolveModerationReportHandler
{
    private readonly IModReportRepository _modReportRepository;
    private readonly IAccountAuthorizationService _authorizationService;

    public ResolveModerationReportHandler(IModReportRepository modReportRepository, IAccountAuthorizationService authorizationService)
    {
        _modReportRepository = modReportRepository;
        _authorizationService = authorizationService;
    }

    public async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid reportId,
        ResolveModerationReportRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (currentUser is null)
        {
            return Results.Unauthorized();
        }

        if (currentUser.IsBanned(DateTimeOffset.UtcNow) || !_authorizationService.HasRole(currentUser, UserRoles.Admin, UserRoles.Moderator))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var report = await _modReportRepository.GetByIdAsync(reportId, cancellationToken);
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

        await _modReportRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new ResolveModerationReportResponse(updated.Id, updated.Status, currentUser.Id, resolvedAt));
    }

    public sealed class ResolveModerationReportRequest
    {
        public string? ResolutionNote { get; init; }
    }
}

public sealed record ResolveModerationReportResponse(Guid ReportId, string Status, Guid ResolvedByUserId, DateTimeOffset ResolvedAt);
