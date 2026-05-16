using Microsoft.AspNetCore.Mvc.RazorPages;
using ModVox.Web.Repositories;

namespace ModVox.Web.Pages;

public sealed class GameModel : PageModel
{
    private readonly IGameRepository _gameRepository;
    private readonly IModRepository _modRepository;

    public GameModel(IGameRepository gameRepository, IModRepository modRepository)
    {
        _gameRepository = gameRepository;
        _modRepository = modRepository;
    }

    public bool IsMissing { get; private set; }
    public string GameName { get; private set; } = string.Empty;
    public string Query { get; private set; } = string.Empty;
    public List<(Guid GameId, Guid ModId, string Label, long DownloadCount)> Mods { get; } = new();

    public async Task OnGetAsync(Guid gameId, string? q, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(gameId, cancellationToken);
        if (game is null || game.IsHidden)
        {
            IsMissing = true;
            return;
        }

        Query = q?.Trim() ?? string.Empty;
        GameName = game.Name;
        var mods = await _modRepository.ListVisibleByGameIdAsync(gameId, cancellationToken);
        var filtered = string.IsNullOrWhiteSpace(Query)
            ? mods
            : mods.Where(x => x.Owner.Contains(Query, StringComparison.OrdinalIgnoreCase) || x.Repository.Contains(Query, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var mod in filtered.OrderByDescending(x => x.DownloadCount))
        {
            Mods.Add((mod.GameId, mod.Id, mod.Owner + "/" + mod.Repository, mod.DownloadCount));
        }
    }
}
