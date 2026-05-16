using ModVox.Web.ApiModels;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Endpoints;

public sealed class ListUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/admin/users", HandleAsync);
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        string? q,
        int? page,
        int? pageSize,
        IUserRepository userRepository,
        IAccountAuthorizationService authorizationService,
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

        var users = await userRepository.ListAsync(cancellationToken);
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
            .Select(x => new UserAccountResponse(x.Id, x.Username, x.DisplayName, x.Email, x.Role, x.MustChangeCredentials))
            .ToList();

        return Results.Ok(new
        {
            page = safePage,
            page_size = safePageSize,
            total_count = totalCount,
            total_pages = totalPages,
            items = pageItems
        });
    }
}
