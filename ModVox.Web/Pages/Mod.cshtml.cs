using Microsoft.AspNetCore.Mvc.RazorPages;
using ModVox.Web.Repositories;

namespace ModVox.Web.Pages;

public sealed class ModModel : PageModel
{
    private readonly IModRepository _modRepository;

    public ModModel(IModRepository modRepository)
    {
        _modRepository = modRepository;
    }

    public bool IsMissing { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Provider { get; private set; } = string.Empty;
    public string ReadmePath { get; private set; } = string.Empty;
    public string ChangelogPath { get; private set; } = string.Empty;

    public async Task OnGetAsync(Guid gameId, Guid modId, CancellationToken cancellationToken)
    {
        var mod = await _modRepository.GetByGameAndIdAsync(gameId, modId, includeHidden: false, cancellationToken);
        if (mod is null)
        {
            IsMissing = true;
            return;
        }

        Name = mod.Owner + "/" + mod.Repository;
        Provider = mod.Provider;
        ReadmePath = mod.ReadmePath;
        ChangelogPath = mod.ChangelogPath;
    }
}
