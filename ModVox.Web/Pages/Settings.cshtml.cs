using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModVox.Web.Security;

namespace ModVox.Web.Pages;

public sealed class SettingsModel : PageModel
{
    private readonly IAccountAuthorizationService _authorizationService;

    public SettingsModel(IAccountAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var user = await _authorizationService.GetCurrentUserAsync(HttpContext, cancellationToken);
        if (user is null)
        {
            return Redirect("/login");
        }

        return Page();
    }
}
