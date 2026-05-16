namespace ModVox.Web.Services;

public sealed class PageIncludeService : IPageIncludeService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IMarkdownRenderer _markdownRenderer;

    public PageIncludeService(IWebHostEnvironment environment, IMarkdownRenderer markdownRenderer)
    {
        _environment = environment;
        _markdownRenderer = markdownRenderer;
    }

    public async Task<string> RenderIncludeAsync(string includeName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(includeName))
        {
            return string.Empty;
        }

        var safe = includeName.Trim().ToLowerInvariant();
        if (safe.Contains("..", StringComparison.Ordinal) || safe.Contains('/', StringComparison.Ordinal) || safe.Contains('\\'))
        {
            return string.Empty;
        }

        var includesDir = Path.Combine(_environment.ContentRootPath, "Pages", "Content", "Includes");
        var filePath = Path.Combine(includesDir, $"{safe}.md");
        if (!File.Exists(filePath))
        {
            return string.Empty;
        }

        var markdown = await File.ReadAllTextAsync(filePath, cancellationToken);
        return _markdownRenderer.RenderToSafeHtml(markdown);
    }
}
