using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.ModsControllerHandlers;

public sealed class CreateModReportHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IModRepository _modRepository;
    private readonly IModReportRepository _modReportRepository;

    public CreateModReportHandler(
        IAccountAuthorizationService authorizationService,
        IModRepository modRepository,
        IModReportRepository modReportRepository)
    {
        _authorizationService = authorizationService;
        _modRepository = modRepository;
        _modReportRepository = modReportRepository;
    }

    public async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid modId,
        CreateModReportRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (currentUser is null)
        {
            return Results.Unauthorized();
        }

        if (currentUser.IsBanned(DateTimeOffset.UtcNow))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var mod = await _modRepository.GetByIdAsync(modId, cancellationToken);
        if (mod is null)
        {
            return Results.NotFound(new { message = "Mod not found." });
        }

        if (!ModReportType.IsAllowed(request.ReportType))
        {
            return Results.BadRequest(new { message = "Invalid report type." });
        }

        var now = DateTimeOffset.UtcNow;
        var report = new ModReport(
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

        await _modReportRepository.AddAsync(report, cancellationToken);
        return Results.Created($"/api/v1/moderation/reports/{report.Id}", new CreateModReportResponse(report.Id, report.ModId, report.ReportType, report.Status));
    }

    public sealed class CreateModReportRequest
    {
        public string ReportType { get; init; } = string.Empty;
        public string Details { get; init; } = string.Empty;
    }
}

public sealed record CreateModReportResponse(Guid ReportId, Guid ModId, string ReportType, string Status);
