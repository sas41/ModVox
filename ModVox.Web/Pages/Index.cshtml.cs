using Microsoft.AspNetCore.Mvc.RazorPages;
using ModVox.Web.Repositories;

namespace ModVox.Web.Pages;

public sealed class IndexModel : PageModel
{
    private readonly IGameRepository _gameRepository;

    public IndexModel(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public string Query { get; private set; } = string.Empty;
    public List<(Guid GameId, string Name)> Items { get; } = new();

    public async Task OnGetAsync(string? q, CancellationToken cancellationToken)
    {
        Query = q?.Trim() ?? string.Empty;
        var games = await _gameRepository.ListAsync(cancellationToken);
        var filtered = string.IsNullOrWhiteSpace(Query)
            ? games.Where(x => !x.IsHidden)
            : games.Where(x => !x.IsHidden && (x.Name.Contains(Query, StringComparison.OrdinalIgnoreCase) || x.Slug.Contains(Query, StringComparison.OrdinalIgnoreCase)));

        foreach (var game in filtered.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase))
        {
            Items.Add((game.Id, game.Name));
        }
    }
}
