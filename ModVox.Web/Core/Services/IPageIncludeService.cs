namespace ModVox.Web.Services;

public interface IPageIncludeService
{
    Task<string> RenderIncludeAsync(string includeName, CancellationToken cancellationToken);
}
