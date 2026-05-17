using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class UpdateDisplayNameHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IUserRepository _userRepository;

    public UpdateDisplayNameHandler(IAccountAuthorizationService authorizationService, IUserRepository userRepository)
    {
        _authorizationService = authorizationService;
        _userRepository = userRepository;
    }

    public async Task<IResult> HandleAsync(HttpContext httpContext, UpdateDisplayNameRequest request, CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(httpContext, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return Results.BadRequest(new { message = "Display name is required." });
        }

        var updated = user with { DisplayName = request.DisplayName.Trim(), UpdatedAt = DateTimeOffset.UtcNow };
        await _userRepository.UpdateAsync(updated, cancellationToken);
        return Results.Ok(new UpdateDisplayNameResponse(updated.DisplayName));
    }

    public sealed class UpdateDisplayNameRequest
    {
        public string DisplayName { get; init; } = string.Empty;
    }
}

public sealed record UpdateDisplayNameResponse(string DisplayName);
