using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints;

public sealed class DeleteAccountEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/account/delete", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        IUserRepository userRepository,
        IAccountAuthorizationService authorizationService,
        IAccountSessionService accountSessionService,
        IAuditLogService auditLogService,
        CancellationToken cancellationToken)
    {
        var user = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var updated = user with
        {
            IsDeleted = true,
            BanType = UserBanTypes.Permanent,
            BanExpiresAt = null,
            SessionVersion = user.SessionVersion + 1,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await userRepository.UpdateAsync(updated, cancellationToken);
        await accountSessionService.LogoutAsync(httpContext, cancellationToken);
        await auditLogService.WriteAsync("account.delete", user.Id, $"User {user.Id} deleted own account.", cancellationToken);
        return Results.NoContent();
    }
}
