namespace ModVox.Web.Services;

public interface IMarkdownRenderer
{
    string RenderToSafeHtml(string markdown);
}
