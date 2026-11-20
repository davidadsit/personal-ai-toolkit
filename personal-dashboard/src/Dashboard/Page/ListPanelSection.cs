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
        {Body(panel)}
        </section>
        """;

    private static string Body(ListPanel panel)
    {
        var feed = panel.Feed;
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
        var groups = rows.OrderBy(SortKey).GroupBy(row => ParseDate(row.Date));
        return refreshed + string.Concat(groups.Select(group => RenderGroup(group, panel.Icons)));
    }

    private static string RenderGroup(IGrouping<DateOnly?, ListPanelRow> group, IReadOnlyDictionary<string, string> icons)
    {
        var heading = group.Key is { } date ? date.ToString("ddd M/d", CultureInfo.InvariantCulture) : "No date";
        return $"""<h3 class="row-date">{heading}</h3><ul>{string.Concat(group.Select(row => RenderRow(row, icons)))}</ul>""";
    }

    private static (DateOnly Date, TimeOnly Time) SortKey(ListPanelRow row) =>
        (ParseDate(row.Date) ?? DateOnly.MaxValue, ParseTime(row.Time) ?? TimeOnly.MinValue);

    private static string RenderRow(ListPanelRow row, IReadOnlyDictionary<string, string> icons)
    {
        var icon = icons.TryGetValue(row.Icon ?? "", out var dataUri) ? $"""<img class="row-icon" src="{Encode(dataUri)}" alt="">""" : "";
        var link = row.Link ?? "";
        var title = link.StartsWith(LinkPrefix, StringComparison.Ordinal)
            ? $"""<a href="{Encode(link)}" target="_blank" rel="noopener">{ToHtml(row.Title ?? "")}</a>"""
            : ToHtml(row.Title ?? "");
        var subtitle = string.IsNullOrEmpty(row.Subtitle) ? "" : $"""<div class="muted">{ToHtml(row.Subtitle)}</div>""";
        return $"""
            <li class="list-row">
            <div class="row-main"><div class="row-title">{icon}{title}</div>{subtitle}</div>
            <div class="row-status">{ToHtml(row.Status ?? "")}</div>
            <div class="row-when">{Time(row)}</div>
            </li>
            """;
    }

    private static string Time(ListPanelRow row) =>
        ParseTime(row.Time) is { } time ? time.ToString("h:mm tt", CultureInfo.InvariantCulture) : "";

    private static DateOnly? ParseDate(string? date) =>
        DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null;

    private static TimeOnly? ParseTime(string? time) =>
        TimeOnly.TryParseExact(time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : null;
}
