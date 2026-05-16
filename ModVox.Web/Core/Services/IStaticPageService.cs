namespace ModVox.Web.Services;

public interface IStaticPageService
{
    Task<string?> RenderPageHtmlAsync(string slug, CancellationToken cancellationToken);
}
