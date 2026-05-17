using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class LoginHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IAccountSessionService _accountSessionService;

    public LoginHandler(IUserRepository userRepository, IPasswordService passwordService, IAccountSessionService accountSessionService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _accountSessionService = accountSessionService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new { message = "Username and password are required." });
        }

        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user is null || !_passwordService.Verify(request.Password, user.PasswordHash))
        {
            return Results.Unauthorized();
        }

        if (user.IsBanned(DateTimeOffset.UtcNow))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        await _accountSessionService.CreateSessionAsync(httpContext, user, cancellationToken);
        return Results.Ok(new LoginResponse(user.Id, user.Username, user.DisplayName, user.Role, user.IsAdmin, user.MustChangeCredentials));
    }

    public sealed class LoginRequest
    {
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}

public sealed record LoginResponse(
    Guid UserId,
    string Username,
    string DisplayName,
    string Role,
    bool IsAdmin,
    bool MustChangeCredentials);
