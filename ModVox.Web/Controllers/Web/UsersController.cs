using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ModVox.Web.Endpoints.UsersControllerHandlers;

namespace ModVox.Web.Endpoints;

[ApiController]
public sealed class UsersController : ControllerBase
{
    private THandler CreateHandler<THandler>() where THandler : notnull =>
        ActivatorUtilities.CreateInstance<THandler>(HttpContext.RequestServices);

    [HttpPost("/api/v1/auth/login")]
    public Task<IResult> LoginAsync(
        LoginHandler.LoginRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<LoginHandler>().HandleAsync(HttpContext, request, cancellationToken);

    [HttpGet("/api/v1/auth/me")]
    public Task<IResult> GetMeAsync(
        CancellationToken cancellationToken) =>
        CreateHandler<GetMeHandler>().HandleAsync(HttpContext, cancellationToken);

    [HttpPost("/api/v1/auth/logout")]
    public Task<IResult> LogoutAsync(
        CancellationToken cancellationToken) =>
        CreateHandler<LogoutHandler>().HandleAsync(HttpContext, cancellationToken);

    [HttpPost("/api/v1/auth/logout-all")]
    public Task<IResult> LogoutAllAsync(
        CancellationToken cancellationToken) =>
        CreateHandler<LogoutAllHandler>().HandleAsync(HttpContext, cancellationToken);

    [HttpPost("/api/v1/auth/change-credentials")]
    public Task<IResult> ChangeCredentialsAsync(
        ChangeCredentialsHandler.ChangeCredentialsRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<ChangeCredentialsHandler>().HandleAsync(HttpContext, request, cancellationToken);

    [HttpPost("/api/v1/account/change-password")]
    public Task<IResult> ChangePasswordAsync(
        ChangePasswordHandler.ChangePasswordRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<ChangePasswordHandler>().HandleAsync(HttpContext, request, cancellationToken);

    [HttpPost("/api/v1/account/change-display-name")]
    public Task<IResult> UpdateDisplayNameAsync(
        UpdateDisplayNameHandler.UpdateDisplayNameRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<UpdateDisplayNameHandler>().HandleAsync(HttpContext, request, cancellationToken);

    [HttpPost("/api/v1/account/delete")]
    public Task<IResult> DeleteAccountAsync(
        CancellationToken cancellationToken) =>
        CreateHandler<DeleteAccountHandler>().HandleAsync(HttpContext, cancellationToken);

    [HttpGet("/api/v1/admin/users")]
    public Task<IResult> ListUsersAsync(
        string? q,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken) =>
        CreateHandler<ListUsersHandler>().HandleAsync(HttpContext, q, page, pageSize, cancellationToken);

    [HttpGet("/api/v1/admin/users/{userId:guid}")]
    public Task<IResult> GetAdminUserAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        CreateHandler<GetAdminUserHandler>().HandleAsync(HttpContext, userId, cancellationToken);

    [HttpPost("/api/v1/admin/users")]
    public Task<IResult> CreateUserAsync(
        CreateUserHandler.CreateUserRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<CreateUserHandler>().HandleAsync(HttpContext, request, cancellationToken);

    [HttpPost("/api/v1/admin/users/{userId:guid}/role")]
    public Task<IResult> UpdateUserRoleAsync(
        Guid userId,
        UpdateUserRoleHandler.UpdateUserRoleRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<UpdateUserRoleHandler>().HandleAsync(HttpContext, userId, request, cancellationToken);

    [HttpPost("/api/v1/admin/users/{userId:guid}/email")]
    public Task<IResult> UpdateUserEmailAsync(
        Guid userId,
        UpdateUserEmailHandler.UpdateUserEmailRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<UpdateUserEmailHandler>().HandleAsync(HttpContext, userId, request, cancellationToken);

    [HttpPost("/api/v1/admin/users/{userId:guid}/password")]
    public Task<IResult> UpdateUserPasswordAsync(
        Guid userId,
        UpdateUserPasswordHandler.UpdateUserPasswordRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<UpdateUserPasswordHandler>().HandleAsync(HttpContext, userId, request, cancellationToken);

    [HttpPost("/api/v1/admin/users/{userId:guid}/username")]
    public Task<IResult> UpdateUserUsernameAsync(
        Guid userId,
        UpdateUserUsernameHandler.UpdateUserUsernameRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<UpdateUserUsernameHandler>().HandleAsync(HttpContext, userId, request, cancellationToken);

    [HttpPost("/api/v1/admin/users/{userId:guid}/display-name")]
    public Task<IResult> UpdateUserDisplayNameAdminAsync(
        Guid userId,
        UpdateUserDisplayNameAdminHandler.UpdateUserDisplayNameAdminRequest request,
        CancellationToken cancellationToken) =>
        CreateHandler<UpdateUserDisplayNameAdminHandler>().HandleAsync(HttpContext, userId, request, cancellationToken);

    [HttpPost("/api/v1/admin/users/{userId:guid}/revoke-all-tokens")]
    public Task<IResult> RevokeUserSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        CreateHandler<RevokeUserSessionsHandler>().HandleAsync(HttpContext, userId, cancellationToken);

    [HttpPost("/api/v1/admin/users/{userId:guid}/mods/keys/revoke-all")]
    public Task<IResult> RevokeAllUserModKeysAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        CreateHandler<RevokeAllUserModKeysHandler>().HandleAsync(HttpContext, userId, cancellationToken);
}
