using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ModVox.Web.Config;

namespace ModVox.Web.Endpoints;

public sealed class GetManifestScaffoldEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/manifest/scaffold", HandleAsync);
    }

    private static IResult HandleAsync(IOptions<ManifestOptions> manifestOptions)
    {
        var fileName = manifestOptions.Value.FileName;

        var scaffold = new
        {
            verify = "",
            name = "My Mod Name",
            description = "A short description of your mod.",
            default_ref = "main",
            readme = "README.md",
            changelog = "CHANGELOG.md",
            images = "images",
            tags = Array.Empty<string>(),
            credits = new { },
            external_credits = new Dictionary<string, string>
            {
                ["Community Contributor"] = "Icon design"
            }
        };

        var json = JsonSerializer.Serialize(scaffold, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var bytes = Encoding.UTF8.GetBytes(json);

        return Results.File(
            bytes,
            contentType: "application/json",
            fileDownloadName: fileName);
    }
}
