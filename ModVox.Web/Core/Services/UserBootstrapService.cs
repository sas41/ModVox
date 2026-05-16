using ModVox.Web.Domain;
using ModVox.Web.Repositories;
using ModVox.Web.Security;

namespace ModVox.Web.Services;

public sealed class UserBootstrapService : IUserBootstrapService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;

    public UserBootstrapService(IUserRepository userRepository, IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
    }

    public async Task EnsureDefaultAdminAsync(CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByUsernameAsync("admin", cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var admin = new UserAccount(
            Guid.NewGuid(),
            "admin",
            "Administrator",
            "admin@local.modvox",
            _passwordService.Hash("admin"),
            Role: UserRoles.Admin,
            MustChangeCredentials: true,
            BanType: UserBanTypes.None,
            BanExpiresAt: null,
            SessionVersion: 1,
            IsDeleted: false,
            now,
            now);

        await _userRepository.AddAsync(admin, cancellationToken);
    }
}
