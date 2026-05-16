namespace ModVox.Web.ApiModels;

public sealed class CreateGameRequest
{
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
