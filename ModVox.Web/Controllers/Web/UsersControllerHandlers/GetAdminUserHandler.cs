using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class GetAdminUserHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IUserRepository _userRepository;

    public GetAdminUserHandler(IAccountAuthorizationService authorizationService, IUserRepository userRepository)
    {
        _authorizationService = authorizationService;
        _userRepository = userRepository;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, Guid userId, CancellationToken cancellationToken)
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

        return Results.Ok(new GetAdminUserResponse(user.Id, user.Username, user.DisplayName, user.Email, user.Role, user.MustChangeCredentials));
    }
}

public sealed record GetAdminUserResponse(
    Guid UserId,
    string Username,
    string DisplayName,
    string Email,
    string Role,
    bool MustChangeCredentials);
