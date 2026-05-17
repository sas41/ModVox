using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class LogoutHandler
{
    private readonly IAccountSessionService _accountSessionService;

    public LogoutHandler(IAccountSessionService accountSessionService)
    {
        _accountSessionService = accountSessionService;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        await _accountSessionService.LogoutAsync(httpContext, cancellationToken);
        return Results.NoContent();
    }
}

public sealed record LogoutResponse(bool NoContent = true);
