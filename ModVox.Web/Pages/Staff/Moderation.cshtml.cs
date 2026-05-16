using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Pages.Staff;

public sealed class ModerationModel : PageModel
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IModRepository _modRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IPageIncludeService _pageIncludeService;

    public ModerationModel(
        IAccountAuthorizationService authorizationService,
        IModRepository modRepository,
        IGameRepository gameRepository,
        IPageIncludeService pageIncludeService)
    {
        _authorizationService = authorizationService;
        _modRepository = modRepository;
        _gameRepository = gameRepository;
        _pageIncludeService = pageIncludeService;
    }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    public bool IsAdmin { get; private set; }
    public string StaffHelpHtml { get; private set; } = string.Empty;
    public List<ModRow> Mods { get; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(HttpContext, cancellationToken);
        if (user is null)
        {
            return Redirect("/login");
        }

        if (!_authorizationService.HasRole(user, UserRoles.Admin, UserRoles.Moderator))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        IsAdmin = _authorizationService.HasRole(user, UserRoles.Admin);

        var games = await _gameRepository.ListAsync(cancellationToken);
        var gameIds = games.Select(x => x.Id).Distinct().ToList();
        var list = new List<ModRecord>();
        foreach (var gameId in gameIds)
        {
            var modsByGame = await _modRepository.ListByGameIdAsync(gameId, cancellationToken);
            list.AddRange(modsByGame);
        }

        IEnumerable<ModRecord> filtered = list
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .OrderByDescending(x => x.UpdatedAt);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var q = Search.Trim();
            filtered = filtered.Where(x =>
                x.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                x.Owner.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                x.Repository.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(StatusFilter))
        {
            filtered = filtered.Where(x => string.Equals(x.ModerationStatus, StatusFilter, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var mod in filtered)
        {
            Mods.Add(new ModRow(mod.Id, mod.Name, mod.Owner, mod.Repository, mod.ModerationStatus, mod.UpdatedAt));
        }

        StaffHelpHtml = await _pageIncludeService.RenderIncludeAsync("staff-help", cancellationToken);
        return Page();
    }

    public sealed record ModRow(
        Guid ModId,
        string Name,
        string Owner,
        string Repository,
        string ModerationStatus,
        DateTimeOffset UpdatedAt);
}
