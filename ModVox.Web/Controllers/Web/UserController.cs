namespace ModVox.Web.Endpoints;

public sealed class UserController : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        new LoginEndpoint().MapEndpoint(app);
        new LogoutEndpoint().MapEndpoint(app);
        new LogoutAllEndpoint().MapEndpoint(app);
        new ChangeCredentialsEndpoint().MapEndpoint(app);
        new GetMeEndpoint().MapEndpoint(app);
        new DeleteAccountEndpoint().MapEndpoint(app);
        new UpdateDisplayNameEndpoint().MapEndpoint(app);
        new ChangePasswordEndpoint().MapEndpoint(app);
        new UpdateUserRoleEndpoint().MapEndpoint(app);
        new UpdateUserEmailEndpoint().MapEndpoint(app);
        new UpdateUserPasswordEndpoint().MapEndpoint(app);
        new RevokeUserSessionsEndpoint().MapEndpoint(app);
        new ListUsersEndpoint().MapEndpoint(app);
        new GetAdminUserEndpoint().MapEndpoint(app);
        new RevokeAllUserModKeysEndpoint().MapEndpoint(app);
        new UpdateUserUsernameEndpoint().MapEndpoint(app);
        new UpdateUserDisplayNameAdminEndpoint().MapEndpoint(app);
        new CreateUserEndpoint().MapEndpoint(app);
    }
}
