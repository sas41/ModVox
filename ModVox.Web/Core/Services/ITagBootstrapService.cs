namespace ModVox.Web.Services;

public interface ITagBootstrapService
{
    Task EnsureSeededAsync(CancellationToken cancellationToken);
}
