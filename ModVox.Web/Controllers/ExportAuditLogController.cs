using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class ExportAuditLogEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/admin/audit/export", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        IAuditLogRepository auditLogRepository,
        IAccountAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var actor = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (actor is null)
        {
            return Results.Unauthorized();
        }

        if (!authorizationService.HasRole(actor, UserRoles.Admin))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        var logs = await auditLogRepository.ListAsync(cancellationToken);
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
