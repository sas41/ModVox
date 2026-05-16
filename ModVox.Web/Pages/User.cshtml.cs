using Microsoft.AspNetCore.Mvc.RazorPages;
using ModVox.Web.Repositories;

namespace ModVox.Web.Pages;

public sealed class UserModel : PageModel
{
    private readonly IUserRepository _userRepository;

    public UserModel(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public bool IsMissing { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;

    public async Task OnGetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null || user.IsDeleted)
        {
            IsMissing = true;
            return;
        }

        Username = user.Username;
        DisplayName = user.DisplayName;
    }
}
