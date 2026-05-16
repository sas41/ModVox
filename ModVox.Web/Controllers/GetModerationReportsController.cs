using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class GetModerationReportsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/moderation/reports", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
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

        var reports = await modReportRepository.ListOpenAsync(cancellationToken);
        var response = reports
            .Select(x => new ModReportItemResponse(x.Id, x.ModId, x.ReporterUserId, x.ReportType, x.Details, x.Status, x.CreatedAt))
            .ToList();

        return Results.Ok(response);
    }
}
