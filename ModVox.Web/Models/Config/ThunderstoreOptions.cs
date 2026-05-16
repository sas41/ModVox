namespace ModVox.Web.Config;

public sealed class ThunderstoreOptions
{
    public const string SectionName = "Thunderstore";

    public string OpenApiUrl { get; set; } = "https://thunderstore.io/api/docs/?format=openapi";
    public string PackageIndexUrl { get; set; } = "https://thunderstore.io/api/experimental/package-index/";
}
