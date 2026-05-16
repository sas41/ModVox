using ModVox.Web.ApiModels;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class ChangePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/account/change-password", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        ChangePasswordRequest request,
        IUserRepository userRepository,
        IPasswordService passwordService,
        IAccountAuthorizationService authorizationService,
        IAccountSessionService accountSessionService,
        CancellationToken cancellationToken)
    {
        var user = await authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        if (!passwordService.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return Results.BadRequest(new { message = "Current password is invalid." });
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            return Results.BadRequest(new { message = "New password must be at least 8 characters." });
        }

        var updated = user with
        {
            PasswordHash = passwordService.Hash(request.NewPassword),
            MustChangeCredentials = false,
            SessionVersion = user.SessionVersion + 1,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await userRepository.UpdateAsync(updated, cancellationToken);
        await accountSessionService.LogoutAsync(httpContext, cancellationToken);
        return Results.NoContent();
    }
}
