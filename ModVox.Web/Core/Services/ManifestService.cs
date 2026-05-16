using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using ModVox.Web.Config;
using ModVox.Web.Manifest;
using ModVox.Web.Providers;
using ModVox.Web.Repositories;

namespace ModVox.Web.Services;

public sealed class ManifestService : IManifestService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IRepositoryProviderRegistry _providerRegistry;
    private readonly ITagRepository _tagRepository;
    private readonly ManifestOptions _options;

    public ManifestService(
        IRepositoryProviderRegistry providerRegistry,
        ITagRepository tagRepository,
        IOptions<ManifestOptions> options)
    {
        _providerRegistry = providerRegistry;
        _tagRepository = tagRepository;
        _options = options.Value;
    }

    public async Task<ManifestReadResult> ReadAsync(
        string provider,
        string owner,
        string repository,
        string refName,
        CancellationToken cancellationToken)
    {
        IRepositoryProvider repoProvider;
        try
        {
            repoProvider = _providerRegistry.Get(provider);
        }
        catch
        {
            return new ManifestReadResult.Invalid($"Unknown provider '{provider}'.");
        }

        var coordinates = new RepositoryCoordinates(provider, owner, repository, refName);
        string? raw;
        try
        {
            raw = await repoProvider.GetFileContentAsync(coordinates, _options.FileName, cancellationToken);
        }
        catch (Exception ex)
        {
            return new ManifestReadResult.Invalid($"Failed to fetch manifest: {ex.Message}");
        }

        if (raw is null)
            return new ManifestReadResult.NotFound();

        JsonNode? root;
        try
        {
            root = JsonNode.Parse(raw);
        }
        catch
        {
            return new ManifestReadResult.Invalid("Manifest file is not valid JSON.");
        }

        if (root is not JsonObject obj)
            return new ManifestReadResult.Invalid("Manifest root must be a JSON object.");

        // Required fields
        var name = obj["name"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(name))
            return new ManifestReadResult.Invalid("'name' is required and must be a non-empty string.");

        var defaultRef = obj["default_ref"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(defaultRef))
            return new ManifestReadResult.Invalid("'default_ref' is required and must be a non-empty string.");

        var readme = obj["readme"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(readme))
            return new ManifestReadResult.Invalid("'readme' is required and must be a non-empty string.");

        var changelog = obj["changelog"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(changelog))
            return new ManifestReadResult.Invalid("'changelog' is required and must be a non-empty string.");

        var images = obj["images"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(images))
            return new ManifestReadResult.Invalid("'images' is required and must be a non-empty string.");

        // Tags — required, at least one must resolve
        var tagsNode = obj["tags"];
        if (tagsNode is not JsonArray tagsArray)
            return new ManifestReadResult.Invalid("'tags' is required and must be a JSON array.");

        var tagLabels = new List<string>();
        foreach (var item in tagsArray)
        {
            var label = item?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(label))
                tagLabels.Add(label);
        }

        if (tagLabels.Count == 0)
            return new ManifestReadResult.Invalid("'tags' must contain at least one non-empty label.");

        var allTags = await _tagRepository.ListAsync(cancellationToken);
        var resolvedTagIds = allTags
            .Where(t => tagLabels.Any(l => string.Equals(l, t.Label, StringComparison.OrdinalIgnoreCase)))
            .Select(t => t.Id)
            .ToArray();

        if (resolvedTagIds.Length == 0)
            return new ManifestReadResult.Invalid("None of the specified tags matched any known tags on this server.");

        // Optional fields
        var description = obj["description"]?.GetValue<string>() ?? string.Empty;
        var verify = obj["verify"]?.GetValue<string>();

        var credits = new Dictionary<string, string>();
        if (obj["credits"] is JsonObject creditsObj)
        {
            foreach (var kvp in creditsObj)
            {
                var creditText = kvp.Value?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(kvp.Key) && !string.IsNullOrWhiteSpace(creditText))
                    credits[kvp.Key] = creditText;
            }
        }

        var manifest = new ModManifest(
            Verify: string.IsNullOrWhiteSpace(verify) ? null : verify,
            Name: name,
            Description: string.IsNullOrWhiteSpace(description) ? null : description,
            DefaultRef: defaultRef,
            Readme: readme,
            Changelog: changelog,
            Images: images,
            Tags: tagLabels,
            Credits: credits);

        return new ManifestReadResult.Valid(manifest, resolvedTagIds);
    }
}
