using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class GetMeHandler
{
    private readonly IAccountAuthorizationService _authorizationService;

    public GetMeHandler(IAccountAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new GetMeResponse(user.Id, user.Username, user.DisplayName, user.Email, user.Role, user.MustChangeCredentials));
    }
}

public sealed record GetMeResponse(
    Guid UserId,
    string Username,
    string DisplayName,
    string Email,
    string Role,
    bool MustChangeCredentials);
