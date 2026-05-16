using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ModVox.Web.Services;

public sealed class MarkdownRenderer : IMarkdownRenderer
{
    private static readonly Regex HeaderRegex = new("^#{1,6}\\s+", RegexOptions.Compiled);
    private static readonly Regex UnorderedListRegex = new("^[-*]\\s+", RegexOptions.Compiled);
    private static readonly Regex OrderedListRegex = new("^\\d+\\.\\s+", RegexOptions.Compiled);
    private static readonly Regex InlineCodeRegex = new("`([^`]+)`", RegexOptions.Compiled);
    private static readonly Regex LinkRegex = new("\\[([^\\]]+)\\]\\(([^)]+)\\)", RegexOptions.Compiled);

    public string RenderToSafeHtml(string markdown)
    {
        var lines = markdown.Replace("\r", string.Empty).Split('\n');
        var sb = new StringBuilder();
        var inUnorderedList = false;
        var inOrderedList = false;

        foreach (var rawLine in lines)
        {
            if (string.IsNullOrWhiteSpace(rawLine))
            {
                CloseLists(sb, ref inUnorderedList, ref inOrderedList);
                continue;
            }

            if (HeaderRegex.IsMatch(rawLine))
            {
                CloseLists(sb, ref inUnorderedList, ref inOrderedList);
                var level = rawLine.TakeWhile(ch => ch == '#').Count();
                level = Math.Clamp(level, 1, 6);
                var text = RenderInline(rawLine.TrimStart('#', ' '));
                sb.Append($"<h{level} class=\"md-heading md-h{level}\">{text}</h{level}>");
                continue;
            }

            if (UnorderedListRegex.IsMatch(rawLine))
            {
                if (inOrderedList)
                {
                    sb.Append("</ol>");
                    inOrderedList = false;
                }

                if (!inUnorderedList)
                {
                    sb.Append("<ul class=\"md-list md-list-unordered\">");
                    inUnorderedList = true;
                }

                var listText = RenderInline(UnorderedListRegex.Replace(rawLine, string.Empty, 1));
                sb.Append($"<li class=\"md-list-item\">{listText}</li>");
                continue;
            }

            if (OrderedListRegex.IsMatch(rawLine))
            {
                if (inUnorderedList)
                {
                    sb.Append("</ul>");
                    inUnorderedList = false;
                }

                if (!inOrderedList)
                {
                    sb.Append("<ol class=\"md-list md-list-ordered\">");
                    inOrderedList = true;
                }

                var listText = OrderedListRegex.Replace(rawLine, string.Empty, 1);
                sb.Append($"<li class=\"md-list-item\">{RenderInline(listText)}</li>");
            }
            else
            {
                CloseLists(sb, ref inUnorderedList, ref inOrderedList);
                sb.Append($"<p class=\"md-paragraph\">{RenderInline(rawLine)}</p>");
            }
        }

        CloseLists(sb, ref inUnorderedList, ref inOrderedList);

        return sb.ToString();
    }

    private static string RenderInline(string raw)
    {
        var encoded = WebUtility.HtmlEncode(raw);
        var withCode = InlineCodeRegex.Replace(encoded, "<code class=\"md-inline-code\">$1</code>");
        var withLinks = LinkRegex.Replace(withCode, "<a class=\"md-link\" href=\"$2\" target=\"_blank\" rel=\"noopener noreferrer\">$1</a>");
        return withLinks;
    }

    private static void CloseLists(StringBuilder sb, ref bool inUnorderedList, ref bool inOrderedList)
    {
        if (inUnorderedList)
        {
            sb.Append("</ul>");
            inUnorderedList = false;
        }

        if (inOrderedList)
        {
            sb.Append("</ol>");
            inOrderedList = false;
        }
    }
}
