using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints.UsersControllerHandlers;

public sealed class ListUsersHandler
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IUserRepository _userRepository;

    public ListUsersHandler(IAccountAuthorizationService authorizationService, IUserRepository userRepository)
    {
        _authorizationService = authorizationService;
        _userRepository = userRepository;
    }

    public async Task<IResult> HandleAsync(
        HttpContext httpContext,
        string? q,
        int? page,
        int? pageSize,
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

        var users = await _userRepository.ListAsync(cancellationToken);
        var filtered = string.IsNullOrWhiteSpace(q)
            ? users
            : users.Where(x =>
                    x.Username.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    x.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    x.Email.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();

        var safePage = Math.Max(1, page ?? 1);
        var safePageSize = Math.Clamp(pageSize ?? 20, 1, 100);
        var totalCount = filtered.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)safePageSize));
        var start = (safePage - 1) * safePageSize;

        var pageItems = filtered
            .OrderBy(x => x.Username, StringComparer.OrdinalIgnoreCase)
            .Skip(start)
            .Take(safePageSize)
            .Select(x => new ListUsersResponse.UserItem(
                x.Id,
                x.Username,
                x.DisplayName,
                x.Email,
                x.Role,
                x.MustChangeCredentials))
            .ToList();

        return Results.Ok(new ListUsersResponse(safePage, safePageSize, totalCount, totalPages, pageItems));
    }
}

public sealed record ListUsersResponse(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    IReadOnlyList<ListUsersResponse.UserItem> Items)
{
    public sealed record UserItem(
        Guid UserId,
        string Username,
        string DisplayName,
        string Email,
        string Role,
        bool MustChangeCredentials);
}
