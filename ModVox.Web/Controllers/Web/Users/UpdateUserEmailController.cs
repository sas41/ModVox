using Microsoft.EntityFrameworkCore;
using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class UpdateUserEmailEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/users/{userId:guid}/email", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        Guid userId,
        UpdateUserEmailRequest request,
        IUserRepository userRepository,
        IAccountAuthorizationService authorizationService,
        IAccountSessionService accountSessionService,
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

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Results.BadRequest(new { message = "Email is required." });
        }

        var normalizedEmail = request.Email.Trim();
        var existingByEmail = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
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
            await userRepository.UpdateAsync(updated, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("ix_users_email", StringComparison.OrdinalIgnoreCase) == true)
        {
            return Results.Conflict(new { message = "Email already exists." });
        }

        await accountSessionService.LogoutAllAsync(updated, cancellationToken);
        return Results.Ok(new UserAccountResponse(updated.Id, updated.Username, updated.DisplayName, updated.Email, updated.Role, updated.MustChangeCredentials));
    }
}
