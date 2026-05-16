using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using ModVox.Web.Config;
using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Pages.Maintainer;

public sealed class RegisterModModel : PageModel
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IGameRepository _gameRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ManifestOptions _manifestOptions;

    public RegisterModModel(
        IAccountAuthorizationService authorizationService,
        IGameRepository gameRepository,
        ITagRepository tagRepository,
        IOptions<ManifestOptions> manifestOptions)
    {
        _authorizationService = authorizationService;
        _gameRepository = gameRepository;
        _tagRepository = tagRepository;
        _manifestOptions = manifestOptions.Value;
    }

    public string ManifestFileName { get; private set; } = string.Empty;
    public List<(Guid GameId, string Name)> Games { get; } = new();
    public List<string> TagLabels { get; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(HttpContext, cancellationToken);
        if (user is null)
            return Redirect("/login");

        if (!_authorizationService.HasRole(user, UserRoles.Maintainer, UserRoles.Admin))
            return StatusCode(StatusCodes.Status403Forbidden);

        ManifestFileName = _manifestOptions.FileName;

        var games = await _gameRepository.ListAsync(cancellationToken);
        foreach (var g in games.Where(g => !g.IsHidden).OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase))
            Games.Add((g.Id, g.Name));

        var tags = await _tagRepository.ListAsync(cancellationToken);
        foreach (var tag in tags.OrderBy(t => t.Label, StringComparer.OrdinalIgnoreCase))
            TagLabels.Add(tag.Label);

        return Page();
    }
}
