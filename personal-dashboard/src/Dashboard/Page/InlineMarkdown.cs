using System.Net;
using System.Text.RegularExpressions;

namespace PersonalDashboard.Page;

public static partial class InlineMarkdown
{
    public static string ToHtml(string text) =>
        string.Concat(CodeSpan().Split(text).Select((part, index) => index % 2 == 1
            ? $"<code>{Encode(part)}</code>"
            : Bold().Replace(Encode(part), "<strong>$1</strong>")));

    public static string Encode(string text) => WebUtility.HtmlEncode(text);

    [GeneratedRegex("`([^`]*)`")]
    private static partial Regex CodeSpan();

    [GeneratedRegex(@"\*\*(.+?)\*\*")]
    private static partial Regex Bold();
}
