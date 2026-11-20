using System.Globalization;
using PersonalDashboard.Feeds;
using PersonalDashboard.Workspace;
using static PersonalDashboard.Page.InlineMarkdown;

namespace PersonalDashboard.Page;

public static class ListPanelSection
{
    private const string LinkPrefix = "https://";

    public static string Render(ListPanel panel) => $"""
        <section id="panel-{Encode(panel.Settings.Feed)}" class="list-panel">
        <h2 class="label">{Encode(panel.Settings.Title ?? "")}</h2>
        {Body(panel.Feed)}
        </section>
        """;

    private static string Body(ListPanelFeed? feed)
    {
        if (feed is null)
        {
            return """<p class="muted">No data yet. Run /dashboard.</p>""";
        }
        var refreshed = $"""<p class="refreshed">Refreshed {Encode(RefreshedTime.ToDisplay(feed.Refreshed ?? ""))}</p>""";
        var rows = feed.Rows ?? [];
        if (rows.Count == 0)
        {
            return $"""{refreshed}<p>Nothing to show.</p>""";
        }
        return $"""{refreshed}<ul>{string.Concat(rows.OrderBy(SortKey).Select(RenderRow))}</ul>""";
    }

    private static (DateOnly Date, TimeOnly Time) SortKey(ListPanelRow row) =>
        (ParseDate(row.Date) ?? DateOnly.MaxValue, ParseTime(row.Time) ?? TimeOnly.MinValue);

    private static string RenderRow(ListPanelRow row)
    {
        var link = row.Link ?? "";
        var title = link.StartsWith(LinkPrefix, StringComparison.Ordinal)
            ? $"""<a href="{Encode(link)}" target="_blank" rel="noopener">{ToHtml(row.Title ?? "")}</a>"""
            : ToHtml(row.Title ?? "");
        var subtitle = string.IsNullOrEmpty(row.Subtitle) ? "" : $"""<div class="muted">{ToHtml(row.Subtitle)}</div>""";
        return $"""
            <li class="list-row">
            <div class="row-main"><div class="row-title">{title}</div>{subtitle}</div>
            <div class="row-status">{ToHtml(row.Status ?? "")}</div>
            <div class="row-when">{When(row)}</div>
            </li>
            """;
    }

    private static string When(ListPanelRow row)
    {
        if (ParseDate(row.Date) is not { } date)
        {
            return Encode(row.Date ?? "");
        }
        var day = date.ToString("ddd M/d", CultureInfo.InvariantCulture);
        return ParseTime(row.Time) is { } time
            ? $"{day} {time.ToString("h:mm tt", CultureInfo.InvariantCulture)}"
            : day;
    }

    private static DateOnly? ParseDate(string? date) =>
        DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null;

    private static TimeOnly? ParseTime(string? time) =>
        TimeOnly.TryParseExact(time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null;
}
