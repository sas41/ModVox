using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints.StaffControllerHandlers;

public sealed class PurgeAuditLogHandler
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IAccountAuthorizationService _authorizationService;

    public PurgeAuditLogHandler(
        IAuditLogRepository auditLogRepository,
        IAuditLogService auditLogService,
        IAccountAuthorizationService authorizationService)
    {
        _auditLogRepository = auditLogRepository;
        _auditLogService = auditLogService;
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

        await _auditLogRepository.PurgeAsync(cancellationToken);
        await _auditLogService.WriteAsync("audit.purge", actor.Id, "Audit log purged by admin.", cancellationToken);
        return Results.NoContent();
    }
}

public sealed record PurgeAuditLogResponse(bool NoContent = true);
