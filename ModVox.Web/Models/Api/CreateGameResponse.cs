namespace ModVox.Web.ApiModels;

public sealed record CreateGameResponse(
    Guid GameId,
    string Slug,
    string Name);
