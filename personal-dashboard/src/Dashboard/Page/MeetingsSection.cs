using PersonalDashboard.Feeds;
using PersonalDashboard.Workspace;
using static PersonalDashboard.Page.InlineMarkdown;

namespace PersonalDashboard.Page;

public static class MeetingsSection
{
    public static string Render(MeetingsFeed? feed, DashboardSettings settings, DateOnly today) => $"""
        <section id="meetings">
        <h2 class="label">Meetings today <a href="{Encode(settings.CalendarUrl)}" target="_blank" rel="noopener">Open calendar</a></h2>
        {Body(feed, settings.TimeZoneInfo, today)}
        </section>
        """;

    private static string Body(MeetingsFeed? feed, TimeZoneInfo timeZone, DateOnly today)
    {
        if (feed is null)
        {
            return """<p class="muted">No data yet. Run /dashboard.</p>""";
        }
        var refreshed = $"""<p class="refreshed">Refreshed {Encode(RefreshedTime.ToDisplay(feed.Refreshed))}</p>""";
        if (feed.Date != today)
        {
            return $"""{refreshed}<p>Not refreshed today. Run /dashboard.</p>""";
        }
        if (feed.Meetings.Count == 0)
        {
            return $"""{refreshed}<p class="muted">No meetings today.</p>""";
        }
        var meetings = feed.Meetings.Select(meeting => RenderMeeting(meeting, feed.Date, timeZone));
        return $"""{refreshed}<ul>{string.Concat(meetings)}</ul><p id="no-more-meetings" class="muted" hidden>No more meetings today.</p>""";
    }

    private static string RenderMeeting(Meeting meeting, DateOnly date, TimeZoneInfo timeZone)
    {
        var start = MeetingTime.ToInstant(date, meeting.Start, timeZone).ToUnixTimeMilliseconds();
        var end = MeetingTime.ToInstant(date, meeting.End, timeZone).ToUnixTimeMilliseconds();
        var id = Encode($"{date:yyyy-MM-dd} {meeting.Start} {meeting.Title}");
        var critical = meeting.Critical
            ? $"""<span class="badge">Critical</span><div class="reason">{Encode(meeting.Reason)}</div>"""
            : "";
        return $"""<li class="meeting{(meeting.Critical ? " critical" : "")}" data-id="{id}" data-start="{start}" data-end="{end}"><span class="time">{MeetingTime.ToDisplay(meeting.Start)}–{MeetingTime.ToDisplay(meeting.End)}</span> {Encode(meeting.Title)}{critical}</li>""";
    }
}
