using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class CreateModReportEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/mods/{modId:guid}/reports", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid modId,
        CreateModReportRequest request,
        IModRepository modRepository,
        IModReportRepository modReportRepository,
        IAccountAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var currentUser = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (currentUser is null)
        {
            return Results.Unauthorized();
        }

        if (currentUser.IsBanned(DateTimeOffset.UtcNow))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var mod = await modRepository.GetByIdAsync(modId, cancellationToken);
        if (mod is null)
        {
            return Results.NotFound(new { message = "Mod not found." });
        }

        if (!ModReportType.IsAllowed(request.ReportType))
        {
            return Results.BadRequest(new { message = "Invalid report type." });
        }

        var now = DateTimeOffset.UtcNow;
        var report = new ModReportRecord(
            Guid.NewGuid(),
            modId,
            currentUser.Id,
            request.ReportType.Trim().ToLowerInvariant(),
            request.Details.Trim(),
            ModReportStatus.Open,
            ResolvedByUserId: null,
            now,
            ResolvedAt: null,
            ResolutionNote: null);

        await modReportRepository.AddAsync(report, cancellationToken);
        return Results.Created($"/api/v1/moderation/reports/{report.Id}", new CreateModReportResponse(report.Id, report.ModId, report.ReportType, report.Status));
    }
}
