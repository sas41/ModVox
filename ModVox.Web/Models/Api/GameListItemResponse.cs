namespace ModVox.Web.ApiModels;

public sealed record GameListItemResponse(
    Guid GameId,
    string Name,
    string Slug);
