using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Pages.Maintainer;

public sealed class MaintainerIndexModel : PageModel
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IModRepository _modRepository;
    private readonly IGameRepository _gameRepository;

    public MaintainerIndexModel(
        IAccountAuthorizationService authorizationService,
        IModRepository modRepository,
        IGameRepository gameRepository)
    {
        _authorizationService = authorizationService;
        _modRepository = modRepository;
        _gameRepository = gameRepository;
    }

    public bool HasBlockedMods { get; private set; }
    public List<MaintainerModItem> Mods { get; } = new();

    public sealed record MaintainerModItem(
        Guid ModId,
        Guid GameId,
        string Name,
        string Owner,
        string Repository,
        string GameName,
        string ModerationStatus,
        bool IsUnverified,
        bool KeyActive,
        long DownloadCount);

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(HttpContext, cancellationToken);
        if (user is null)
            return Redirect("/login");

        if (!_authorizationService.HasRole(user, UserRoles.Maintainer, UserRoles.Admin))
            return StatusCode(StatusCodes.Status403Forbidden);

        var mods = await _modRepository.ListByMaintainerUserIdAsync(user.Id, cancellationToken);
        HasBlockedMods = mods.Any(m =>
            string.Equals(m.ModerationStatus, ModModerationStatus.Hidden, StringComparison.OrdinalIgnoreCase));

        var games = await _gameRepository.ListAsync(cancellationToken);
        var gameMap = games.ToDictionary(g => g.Id, g => g.Name);

        foreach (var mod in mods)
        {
            gameMap.TryGetValue(mod.GameId, out var gameName);
            Mods.Add(new MaintainerModItem(
                mod.Id,
                mod.GameId,
                mod.Name,
                mod.Owner,
                mod.Repository,
                gameName ?? mod.GameId.ToString(),
                mod.ModerationStatus,
                string.Equals(mod.ModerationStatus, ModModerationStatus.Unverified, StringComparison.OrdinalIgnoreCase),
                mod.KeyHash is not null,
                mod.DownloadCount));
        }

        return Page();
    }
}
