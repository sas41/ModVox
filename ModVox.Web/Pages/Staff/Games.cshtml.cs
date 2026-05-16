using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModVox.Web.Domain;
using ModVox.Web.Security;
using ModVox.Web.Services;

namespace ModVox.Web.Pages.Staff;

public sealed class GamesModel : PageModel
{
    private readonly IAccountAuthorizationService _authorizationService;
    private readonly IPageIncludeService _pageIncludeService;

    public GamesModel(IAccountAuthorizationService authorizationService, IPageIncludeService pageIncludeService)
    {
        _authorizationService = authorizationService;
        _pageIncludeService = pageIncludeService;
    }

    public string StaffHelpHtml { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(HttpContext, cancellationToken);
        if (user is null)
        {
            return Redirect("/login");
        }

        if (!_authorizationService.HasRole(user, UserRoles.Admin))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        StaffHelpHtml = await _pageIncludeService.RenderIncludeAsync("staff-help", cancellationToken);
        return Page();
    }
}
