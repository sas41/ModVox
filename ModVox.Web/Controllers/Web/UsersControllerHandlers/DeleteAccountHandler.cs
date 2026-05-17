using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class DeleteAccountHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IUserRepository _userRepository;
    private readonly IAccountSessionService _accountSessionService;
    private readonly IAuditLogService _auditLogService;

    public DeleteAccountHandler(
        IAccountAuthorizationService authorizationService,
        IUserRepository userRepository,
        IAccountSessionService accountSessionService,
        IAuditLogService auditLogService)
    {
        _authorizationService = authorizationService;
        _userRepository = userRepository;
        _accountSessionService = accountSessionService;
        _auditLogService = auditLogService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
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

        await _userRepository.UpdateAsync(updated, cancellationToken);
        await _accountSessionService.LogoutAsync(httpContext, cancellationToken);
        await _auditLogService.WriteAsync("account.delete", user.Id, $"User {user.Id} deleted own account.", cancellationToken);
        return Results.NoContent();
    }
}

public sealed record DeleteAccountResponse(bool NoContent = true);
