namespace ModVox.Web.Services;

public interface IUserBootstrapService
{
    Task EnsureDefaultAdminAsync(CancellationToken cancellationToken);
}
