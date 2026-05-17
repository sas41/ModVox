using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.StaffControllerHandlers;

public sealed class GetModerationReportsHandler
{
    private readonly IModReportRepository _modReportRepository;
    private readonly IAccountAuthorizationService _authorizationService;

    public GetModerationReportsHandler(IModReportRepository modReportRepository, IAccountAuthorizationService authorizationService)
    {
        _modReportRepository = modReportRepository;
        _authorizationService = authorizationService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, CancellationToken cancellationToken)
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

        var reports = await _modReportRepository.ListOpenAsync(cancellationToken);
        var response = reports
            .Select(x => new GetModerationReportsResponse(x.Id, x.ModId, x.ReporterUserId, x.ReportType, x.Details, x.Status, x.CreatedAt))
            .ToList();

        return Results.Ok(response);
    }
}

public sealed record GetModerationReportsResponse(
    Guid ReportId,
    Guid ModId,
    Guid ReporterUserId,
    string ReportType,
    string Details,
    string Status,
    DateTimeOffset CreatedAt);
