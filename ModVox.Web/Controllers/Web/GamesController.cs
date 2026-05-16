namespace ModVox.Web.Endpoints;

public sealed class GamesController : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        new CreateGameEndpoint().MapEndpoint(app);
        new ListGamesEndpoint().MapEndpoint(app);
        new ListAdminGamesEndpoint().MapEndpoint(app);
        new GetAdminGameEndpoint().MapEndpoint(app);
        new UpdateGameEndpoint().MapEndpoint(app);
    }
}
