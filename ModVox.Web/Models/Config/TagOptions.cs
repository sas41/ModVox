namespace ModVox.Web.Config;

public sealed class TagOptions
{
    public const string SectionName = "Tags";

    public List<string> DefaultSeedLabels { get; init; } = new();
}
