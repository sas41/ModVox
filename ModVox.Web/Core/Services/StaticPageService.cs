namespace ModVox.Web.Services;

public sealed class StaticPageService : IStaticPageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IMarkdownRenderer _markdownRenderer;

    public StaticPageService(IWebHostEnvironment environment, IMarkdownRenderer markdownRenderer)
    {
        _environment = environment;
        _markdownRenderer = markdownRenderer;
    }

    public async Task<string?> RenderPageHtmlAsync(string slug, CancellationToken cancellationToken)
    {
        var safeSlug = string.IsNullOrWhiteSpace(slug) ? "index" : slug.Trim().ToLowerInvariant();
        safeSlug = safeSlug.Replace('\\', '/');
        if (safeSlug.StartsWith("/", StringComparison.Ordinal))
        {
            safeSlug = safeSlug.TrimStart('/');
        }

        if (safeSlug.Contains("..", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(safeSlug))
        {
            return null;
        }

        var pagesDir = Path.Combine(_environment.ContentRootPath, "Pages", "Content");
        var filePath = Path.Combine(pagesDir, $"{safeSlug}.md");
        if (!File.Exists(filePath))
        {
            return null;
        }

        var markdown = await File.ReadAllTextAsync(filePath, cancellationToken);
        var contentHtml = _markdownRenderer.RenderToSafeHtml(markdown);
        var title = safeSlug == "index" ? "ModVox" : $"ModVox - {safeSlug.Replace('/', ' ')}";

        return $@"<!doctype html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>{title}</title>
  <link rel=""stylesheet"" href=""/css/site.css"" />
</head>
<body class=""site-body"">
  <header class=""site-header"">
    <div class=""site-shell"">
      <a class=""brand"" href=""/"">ModVox</a>
      <span class=""brand-subtitle"">Public git mod discovery</span>
    </div>
  </header>
  <main class=""site-main"">
    <article class=""md-doc site-shell"">
      {contentHtml}
    </article>
  </main>
</body>
</html>";
    }
}
