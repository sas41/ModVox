using Microsoft.Extensions.Options;
using ModVox.Web.Config;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;

namespace ModVox.Web.Services;

public sealed class TagBootstrapService : ITagBootstrapService
{
    private readonly ITagRepository _tagRepository;
    private readonly IOptions<TagOptions> _tagOptions;

    public TagBootstrapService(ITagRepository tagRepository, IOptions<TagOptions> tagOptions)
    {
        _tagRepository = tagRepository;
        _tagOptions = tagOptions;
    }

    public async Task EnsureSeededAsync(CancellationToken cancellationToken)
    {
        var existing = await _tagRepository.ListAsync(cancellationToken);
        if (existing.Count > 0)
        {
            return;
        }

        var labels = _tagOptions.Value.DefaultSeedLabels
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var now = DateTimeOffset.UtcNow;
        foreach (var label in labels)
        {
            await _tagRepository.AddAsync(new TagRecord(Guid.NewGuid(), label, now, now), cancellationToken);
        }
    }
}
