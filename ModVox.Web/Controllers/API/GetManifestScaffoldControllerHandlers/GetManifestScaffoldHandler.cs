using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ModVox.Web.Config;

namespace ModVox.Web.Endpoints.GetManifestScaffoldControllerHandlers;

public sealed class GetManifestScaffoldHandler
{
    private readonly ManifestOptions _manifestOptions;

    public GetManifestScaffoldHandler(IOptions<ManifestOptions> manifestOptions)
    {
        _manifestOptions = manifestOptions.Value;
    }

    public IResult Handle()
    {
        var scaffold = new ManifestScaffoldResponse(
            Verify: string.Empty,
            Name: "My Mod Name",
            Description: "A short description of your mod.",
            DefaultRef: "main",
            Readme: "README.md",
            Changelog: "CHANGELOG.md",
            Images: "images",
            Tags: Array.Empty<string>(),
            Credits: new Dictionary<string, string>(),
            ExternalCredits: new Dictionary<string, string>
            {
                ["Community Contributor"] = "Icon design"
            });

        var json = JsonSerializer.Serialize(scaffold, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var bytes = Encoding.UTF8.GetBytes(json);

        return Results.File(
            bytes,
            contentType: "application/json",
            fileDownloadName: _manifestOptions.FileName);
    }
}

public sealed record ManifestScaffoldResponse(
    string Verify,
    string Name,
    string Description,
    string DefaultRef,
    string Readme,
    string Changelog,
    string Images,
    IReadOnlyList<string> Tags,
    IReadOnlyDictionary<string, string> Credits,
    IReadOnlyDictionary<string, string> ExternalCredits);
