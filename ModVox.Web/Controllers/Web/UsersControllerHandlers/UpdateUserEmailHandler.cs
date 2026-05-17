using Microsoft.EntityFrameworkCore;
using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class UpdateUserEmailHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IUserRepository _userRepository;
    private readonly IAccountSessionService _accountSessionService;

    public UpdateUserEmailHandler(
        IAccountAuthorizationService authorizationService,
        IUserRepository userRepository,
        IAccountSessionService accountSessionService)
    {
        _authorizationService = authorizationService;
        _userRepository = userRepository;
        _accountSessionService = accountSessionService;
    }

    public async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid userId,
        UpdateUserEmailRequest request,
        CancellationToken cancellationToken)
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

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Results.BadRequest(new { message = "Email is required." });
        }

        var normalizedEmail = request.Email.Trim();
        var existingByEmail = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingByEmail is not null && existingByEmail.Id != user.Id)
        {
            return Results.Conflict(new { message = "Email already exists." });
        }

        var updated = user with
        {
            Email = normalizedEmail,
            SessionVersion = user.SessionVersion + 1,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        try
        {
            await _userRepository.UpdateAsync(updated, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("ix_users_email", StringComparison.OrdinalIgnoreCase) == true)
        {
            return Results.Conflict(new { message = "Email already exists." });
        }

        await _accountSessionService.LogoutAllAsync(updated, cancellationToken);
        return Results.Ok(new UpdateUserEmailResponse(updated.Id, updated.Username, updated.DisplayName, updated.Email, updated.Role, updated.MustChangeCredentials));
    }

    public sealed class UpdateUserEmailRequest
    {
        public string Email { get; init; } = string.Empty;
    }
}

public sealed record UpdateUserEmailResponse(
    Guid UserId,
    string Username,
    string DisplayName,
    string Email,
    string Role,
    bool MustChangeCredentials);
