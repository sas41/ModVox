using ModVox.Web.ApiModels;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/login", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        LoginRequest request,
        IUserRepository userRepository,
        IPasswordService passwordService,
        IAccountSessionService accountSessionService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new { message = "Username and password are required." });
        }

        var user = await userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user is null || !passwordService.Verify(request.Password, user.PasswordHash))
        {
            return Results.Unauthorized();
        }

        if (user.IsBanned(DateTimeOffset.UtcNow))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        await accountSessionService.CreateSessionAsync(httpContext, user, cancellationToken);
        return Results.Ok(new LoginResponse(user.Id, user.Username, user.DisplayName, user.Role, user.IsAdmin, user.MustChangeCredentials));
    }
}
