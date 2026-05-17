using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.StaffControllerHandlers;

public sealed class ExportAuditLogHandler
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IAccountAuthorizationService _authorizationService;

    public ExportAuditLogHandler(IAuditLogRepository auditLogRepository, IAccountAuthorizationService authorizationService)
    {
        _auditLogRepository = auditLogRepository;
        _authorizationService = authorizationService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var actor = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (actor is null)
        {
            return Results.Unauthorized();
        }

        if (!_authorizationService.HasRole(actor, UserRoles.Admin))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var logs = await _auditLogRepository.ListAsync(cancellationToken);
        var csv = "id,created_at,event_type,actor_user_id,description\n" +
                  string.Join('\n', logs.Select(x =>
                      $"{x.Id},{x.CreatedAt:O},{Escape(x.EventType)},{x.ActorUserId?.ToString() ?? string.Empty},{Escape(x.Description)}"));

        return Results.Text(csv, "text/csv");
    }

    private static string Escape(string value)
    {
        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}

public sealed record ExportAuditLogResponse(string CsvContentType = "text/csv");
